using BoxOptions.Common.Models;
using BoxOptions.Core.Interfaces;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Globalization;

namespace BoxOptions.AzureRepositories.Entities
{
    public class LogEntity : TableEntity, ILogItem
    {
        public string ClientId { get; set; }
        public string EventCode { get; set; }
        public string Message { get; set; }
        public double AccountDelta { get; set; }

        public DateTime Date => Timestamp.DateTime;

        public static string GetPartitionKey(string clientId, DateTime date)
        {
            return string.Format("{0}_{1}", clientId, date.ToString("yyyyMMdd"));
        }

        public static LogEntity CreateEntity(ILogItem src)
        {
            return new LogEntity
            {
                PartitionKey = GetPartitionKey(src.ClientId, DateTime.UtcNow),
                ClientId = src.ClientId,
                EventCode = src.EventCode,
                Message = src.Message,
                AccountDelta = src.AccountDelta
            };
        }

        public static ILogItem CreateDto(LogEntity src)
        {
            return new LogItem
            {
                ClientId = src.ClientId,
                EventCode = src.EventCode,
                Message = src.Message,
                AccountDelta = src.AccountDelta,
                Date = src.Timestamp.DateTime
            };
        }
    }
}
