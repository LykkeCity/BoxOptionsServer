using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using AzureStorage;
using AzureStorage.Tables;
using BoxOptions.Core;
using Microsoft.WindowsAzure.Storage.Table;

namespace BoxOptions.AzureRepositories
{
    public class LogEntity : TableEntity, ILogItem
    {
        public string ClientId { get; set; }
        public string EventCode { get; set; }
        public string Message { get; set; }

        public static string GetPartitionKey(string clientId)
        {
            return clientId;
        }

        public static LogEntity Create(ILogItem src)
        {
            return new LogEntity
            {
                PartitionKey = GetPartitionKey(src.ClientId),
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
            var entities = (await _storage.GetDataAsync(new[] { clientId }, int.MaxValue,
                    entity => entity.Timestamp >= dateFrom && entity.Timestamp < dateTo))
                .OrderByDescending(item => item.Timestamp);

            return entities.Select(LogEntity.CreateLogItem);
        }
    }
}
