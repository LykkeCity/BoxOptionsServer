using System;
using BoxOptions.Core.Interfaces;

namespace BoxOptions.Common.Models
{
    public class LogItem : ILogItem
    {
        public string ClientId { get; set; }
        public string EventCode { get; set; }
        public string Message { get; set; }        
        public double AccountDelta { get; set; }
        public DateTime Date { get; set; }
    }
}
