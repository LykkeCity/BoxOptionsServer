using BoxOptions.Core.Interfaces;
using System;

namespace BoxOptions.Common.Models
{
    public class UserItem : IUserItem
    {
        public string UserId { get; set; }
        public string Balance { get; set; }
        public int CurrentState { get; set; }        
        public string ServerTimestamp { get; set; }
        public DateTime LastChange { get; set; }
    }
}
