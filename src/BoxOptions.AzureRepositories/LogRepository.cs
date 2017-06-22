using AzureStorage;
using AzureStorage.Tables;
using BoxOptions.Core;
using BoxOptions.Core.Interfaces;
using BoxOptions.Core.Models;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace BoxOptions.AzureRepositories
{
    public class LogEntity : TableEntity, ILogItem
    {
        public string ClientId { get; set; }
        public string EventCode { get; set; }
        public string Message { get; set; }

        public static string GetPartitionKey(string clientId, DateTime date)
        {
            return string.Format("{0}_{1}", clientId, date.ToString("yyyyMMdd"));
        }

        public static LogEntity Create(ILogItem src)
        {
            return new LogEntity
            {
                PartitionKey = GetPartitionKey(src.ClientId, DateTime.UtcNow),
                ClientId = src.ClientId,
                EventCode = src.EventCode,
                Message = src.Message
            };
        }

        public static LogItem CreateLogItem(LogEntity src)
        {
            return new LogItem
            {
                ClientId = src.ClientId,
                EventCode = src.EventCode,
                Message = src.Message,
                Timestamp = src.Timestamp.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture)
            };
        }
    }

    public class LogRepository : ILogRepository
    {
        private readonly AzureTableStorage<LogEntity> _storage;

        public LogRepository(AzureTableStorage<LogEntity> storage)
        {
            _storage = storage;
        }

        public async Task InsertAsync(ILogItem olapEntity)
        {
            await _storage.InsertAndGenerateRowKeyAsDateTimeAsync(LogEntity.Create(olapEntity), DateTime.UtcNow);
        }

        public async Task<IEnumerable<LogItem>> GetRange(DateTime dateFrom, DateTime dateTo, string clientId)
        {
            DateTime startDate = dateFrom.Date;
            DateTime endDate = dateTo.Date.AddDays(1);
            DateTime currentDate = startDate;

            List<LogEntity> retval = new List<LogEntity>();

            do
            {
                string partitionKey = LogEntity.GetPartitionKey(clientId, currentDate);
                var entities = (await _storage.GetDataAsync(new[] { partitionKey }, int.MaxValue))
                .OrderByDescending(item => item.Timestamp);
                retval.AddRange(entities);

                currentDate = currentDate.AddDays(1);
            } while (currentDate < endDate);

            //var entities = (await _storage.GetDataAsync(new[] { clientId }, int.MaxValue,
            //        entity => entity.Timestamp >= dateFrom && entity.Timestamp < dateTo))
            //    .OrderByDescending(item => item.Timestamp);

            return retval.Select(LogEntity.CreateLogItem);
        }

        public async Task<IEnumerable<string>> GetClients()
        {
            System.Collections.Concurrent.ConcurrentDictionary<string, byte> partitionKeys = new System.Collections.Concurrent.ConcurrentDictionary<string, byte>();
            await _storage.ExecuteAsync(new TableQuery<LogEntity>(), entity =>
            {
                //foreach (var et in entity.Where(m => m.Timestamp >= dateFrom && m.Timestamp < dateTo).Select(m => m.PartitionKey))
                foreach (var et in entity.Select(m => m.PartitionKey))
                    partitionKeys.TryAdd(et, 0);
            });
            return partitionKeys.Select(m => m.Key);
        }

        public async Task<IEnumerable<LogItem>> GetAll(DateTime dateFrom, DateTime dateTo)
        {
            List<string> clientList = new List<string>(await GetClients());

            List<LogEntity> logs = new List<LogEntity>();

            foreach (var clientId in clientList)
            {
                var entities = (await _storage.GetDataAsync(new[] { clientId }, int.MaxValue,
                    entity => entity.Timestamp >= dateFrom && entity.Timestamp < dateTo));

                logs.AddRange(entities);
            }
            return logs.OrderByDescending(m => m.Timestamp).Select(LogEntity.CreateLogItem);

        }
    }
}
