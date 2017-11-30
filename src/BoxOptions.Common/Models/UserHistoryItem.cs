using System;
using BoxOptions.Core.Interfaces;

namespace BoxOptions.Common.Models
{
    public class UserHistoryItem : IUserHistoryItem
    {   
        public DateTime Date { get; set; }
        public string Message { get; set; }
        public string UserId { get; set; }
        public double AccountDelta { get; set; }
        public GameStatus GameStatus { get; set; }

        int IUserHistoryItem.GameStatus => (int)GameStatus;
    }
}
