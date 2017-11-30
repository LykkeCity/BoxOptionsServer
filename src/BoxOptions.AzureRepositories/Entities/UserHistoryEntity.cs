using BoxOptions.Common.Models;
using BoxOptions.Core.Interfaces;
using Microsoft.WindowsAzure.Storage.Table;
using System;

namespace BoxOptions.AzureRepositories.Entities
{
    public class UserHistoryEntity : TableEntity, IUserHistoryItem
    {
        public string UserId { get; set; }
        public string Status { get; set; }
        public DateTime Date { get; set; }
        public string Message { get; set; }
        public double AccountDelta { get; set; }
        public int GameStatus => int.Parse(Status);

        static int rowKeyCtr = 0;

        public static string GetPartitionKey(string userId, DateTime date)
        {
            return string.Format("{0}_{1}", userId, date.ToString("yyyyMMdd"));
        }

        public static string GetRowKey(DateTime date)
        {
            if (rowKeyCtr >= 999)
                rowKeyCtr = 0;
            return string.Format("{0}.{1}", date.ToString("yyyyMMdd_HHmmss"), rowKeyCtr++.ToString("D3"));
        }


        public static UserHistoryEntity CreateEntity(IUserHistoryItem src)
        {
            return new UserHistoryEntity
            {
                PartitionKey = GetPartitionKey(src.UserId, DateTime.UtcNow),
                RowKey = GetRowKey(src.Date),
                UserId = src.UserId,
                Date = src.Date,
                Status = src.GameStatus.ToString(),
                Message = src.Message,
                AccountDelta = src.AccountDelta
            };
        }

        public static IUserHistoryItem CreateDto(UserHistoryEntity src)
        {
            if (src == null)
                return null;
            return new UserHistoryItem
            {
                UserId = src.UserId,
                Date = src.Date,
                GameStatus = (Common.GameStatus)src.GameStatus,
                Message = src.Message,
                AccountDelta = src.AccountDelta
            };
        }
    }
}
