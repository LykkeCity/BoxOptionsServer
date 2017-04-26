using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BoxOptions.Core
{
    public interface ILogItem
    {
        string ClientId { get; }
        string EventCode { get; }
        string Message { get; }
    }

    public class LogItem : ILogItem
    {
        public string ClientId { get; set; }
        public string EventCode { get; set; }
        public string Message { get; set; }
        public string Timestamp { get; set; }
    }

    public interface ILogRepository
    {
        Task InsertAsync(ILogItem olapEntity);
        Task<IEnumerable<LogItem>> GetRange(DateTime dateFrom, DateTime dateTo, string clientId);
    }
}
