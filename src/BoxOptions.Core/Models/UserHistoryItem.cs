using System;
using BoxOptions.Core.Interfaces;

namespace BoxOptions.Core.Models
{
    public class UserHistoryItem : IUserHistoryItem
    {
        public string Status { get; set; }
        public DateTime Date { get; set; }
        public string Message { get; set; }
        public string UserId { get; set; }
        public double AccountDelta { get; set; }
    }
}
