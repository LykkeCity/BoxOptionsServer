using AzureStorage;
using BoxOptions.AzureRepositories.Entities;
using BoxOptions.Core;
using BoxOptions.Core.Interfaces;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BoxOptions.AzureRepositories
{
    public class LogRepository : ILogRepository
    {
        private readonly INoSQLTableStorage<LogEntity> _storage;

        public LogRepository(INoSQLTableStorage<LogEntity> storage)
        {
            _storage = storage;
        }

        public async Task InsertAsync(ILogItem olapEntity)
        {
            await _storage.InsertAndGenerateRowKeyAsDateTimeAsync(LogEntity.CreateEntity(olapEntity), DateTime.UtcNow);
        }

        public async Task<IEnumerable<ILogItem>> GetRange(DateTime dateFrom, DateTime dateTo, string clientId)
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

            return retval.Select(LogEntity.CreateDto);
        }

        public async Task<IEnumerable<string>> GetClients()
        {
            System.Collections.Concurrent.ConcurrentDictionary<string, byte> partitionKeys = new System.Collections.Concurrent.ConcurrentDictionary<string, byte>();
            await _storage.ExecuteAsync(new TableQuery<LogEntity>(), entity =>
            {
                foreach (var et in entity.Select(m => m.PartitionKey))
                    partitionKeys.TryAdd(et, 0);
            });
            return partitionKeys.Select(m => m.Key);
        }

        public async Task<IEnumerable<ILogItem>> GetAll(DateTime dateFrom, DateTime dateTo)
        {
            List<string> clientList = new List<string>(await GetClients());

            List<LogEntity> logs = new List<LogEntity>();

            foreach (var clientId in clientList)
            {
                var entities = (await _storage.GetDataAsync(new[] { clientId }, int.MaxValue,
                    entity => entity.Timestamp >= dateFrom && entity.Timestamp < dateTo));

                logs.AddRange(entities);
            }
            return logs.OrderByDescending(m => m.Timestamp).Select(LogEntity.CreateDto);

        }
    }
}
