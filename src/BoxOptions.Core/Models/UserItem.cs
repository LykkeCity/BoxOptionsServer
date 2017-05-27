using System;

namespace BoxOptions.Core.Models
{
    public class UserItem : Interfaces.IUserItem
    {
        public string UserId { get; set; }
        public string Balance { get; set; }
        public int CurrentState { get; set; }
        public string CurrentGameId { get; set; }
        public string ServerTimestamp { get; set; }
        public DateTime LastChange { get; set; }
    }
}
