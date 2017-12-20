using BoxOptions.Common.Models;
using BoxOptions.Core.Interfaces;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Globalization;

namespace BoxOptions.AzureRepositories.Entities
{
    public class UserEntity : TableEntity, IUserItem
    {
        public string UserId { get; set; }
        public string Balance { get; set; }
        public int CurrentState { get; set; }
        public DateTime LastChange { get; set; }

        public static string GetPartitionKey(string userId)
        {
            return userId;
        }

        public static UserEntity CreateEntity(IUserItem src)
        {
            return new UserEntity
            {
                PartitionKey = GetPartitionKey(src.UserId),
                RowKey = "User",
                UserId = src.UserId,
                Balance = src.Balance,
                CurrentState = src.CurrentState,
                LastChange = src.LastChange
            };
        }

        public static IUserItem CreateDto(UserEntity src)
        {
            if (src == null)
                return null;
            return new UserItem
            {
                UserId = src.UserId,
                Balance = src.Balance,
                CurrentState = src.CurrentState,
                LastChange = src.LastChange,
                ServerTimestamp = src.Timestamp.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture)
            };
        }
    }
}
