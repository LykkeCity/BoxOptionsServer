using System;

namespace BoxOptions.Core.Models
{
    public class LogItem : Interfaces.ILogItem
    {
        public string ClientId { get; set; }
        public string EventCode { get; set; }
        public string Message { get; set; }
        public string Timestamp { get; set; }
        public double AccountDelta { get; set; }
    }
}
