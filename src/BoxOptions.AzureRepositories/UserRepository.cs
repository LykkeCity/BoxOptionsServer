using AzureStorage;
using AzureStorage.Tables;
using BoxOptions.Core;
using BoxOptions.Core.Interfaces;
using BoxOptions.Core.Models;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace BoxOptions.AzureRepositories
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

        public static UserEntity Create(IUserItem src)
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

        public static UserItem CreateUserItem(UserEntity src)
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
    
    public class UserHistoryEntity : TableEntity, IUserHistoryItem
    {
        public string UserId { get; set; }
        public string Status { get; set; }
        public DateTime Date { get; set; }
        public string Message { get; set; }

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


        public static UserHistoryEntity Create(IUserHistoryItem src)
        {
            return new UserHistoryEntity
            {
                PartitionKey = GetPartitionKey(src.UserId,DateTime.UtcNow),
                RowKey = GetRowKey(src.Date),
                UserId = src.UserId,
                Date = src.Date,
                Status = src.Status,
                Message = src.Message
            };
        }

        public static UserHistoryItem CreateUserHistoryItem(UserHistoryEntity src)
        {
            if (src == null)
                return null;
            return new UserHistoryItem
            {
                UserId = src.UserId,
                Date = src.Date,
                Status = src.Status,
                Message = src.Message
            };
        }
    }
    
    public class UserRepository : IUserRepository
    {
        private readonly AzureTableStorage<UserEntity> _storage;        
        private readonly AzureTableStorage<UserHistoryEntity> _hstorage;

        public UserRepository(AzureTableStorage<UserEntity> storage, AzureTableStorage<UserHistoryEntity> hstorage)
        {
            _storage = storage;            
            _hstorage = hstorage;
        }

        public async Task InsertUserAsync(IEnumerable<IUserItem> olapEntity)
        {
            var total = olapEntity.Select(UserEntity.Create);
            // Group by partition key
            var grouping = from e in total
                           group e by new { e.PartitionKey } into cms
                           select new { key = cms.Key, val = cms.ToList() };


            // Insert grouped baches 
            foreach (var item in grouping)
            {
                var list = item.val;
                do
                {
                    int bufferLen = 128;
                    if (list.Count < 128)
                        bufferLen = list.Count;
                    var buffer = list.Take(bufferLen);
                    await _storage.InsertOrReplaceBatchAsync(buffer);
                    list.RemoveRange(0, bufferLen);

                } while (list.Count > 0);
            }
        }

        public async Task<UserItem> GetUser(string userId)
        {
            var entities = (await _storage.GetDataAsync(new[] { userId }, int.MaxValue,
                entity => entity.RowKey == "User"));
            return UserEntity.CreateUserItem(entities.FirstOrDefault());
                
        }
        
        public async Task InsertHistoryAsync(IEnumerable<IUserHistoryItem> olapEntitiy)
        {
            var total = olapEntitiy.Select(UserHistoryEntity.Create);
            
            // Group by partition key
            var grouping = from e in total
                           group e by new { e.PartitionKey } into cms
                           select new { key = cms.Key, val = cms.ToList() };


            // Insert grouped baches 
            foreach (var item in grouping)
            {
                var list = item.val;
                do
                {
                    int bufferLen = 128;
                    if (list.Count < 128)
                        bufferLen = list.Count;
                    var buffer = list.Take(bufferLen);
                    await _hstorage.InsertOrReplaceBatchAsync(buffer);
                    list.RemoveRange(0, bufferLen);

                } while (list.Count > 0);
            }
        }

        public async Task<IEnumerable<UserHistoryItem>> GetUserHistory(string userId, DateTime dateFrom, DateTime dateTo)
        {

            DateTime startDate = dateFrom.Date;
            DateTime endDate = dateTo.Date.AddDays(1);
            DateTime currentDate = startDate;

            List<UserHistoryEntity> retval = new List<UserHistoryEntity>();
            do
            {
                string partitionKey = UserHistoryEntity.GetPartitionKey(userId, currentDate);
                var entities = (await _hstorage.GetDataAsync(new[] { partitionKey }, int.MaxValue))
                .OrderByDescending(item => item.Timestamp);
                retval.AddRange(entities);

                currentDate = currentDate.AddDays(1);
            } while (currentDate < endDate);
                        

            return retval.Select(UserHistoryEntity.CreateUserHistoryItem);

            //var entities = (await _hstorage.GetDataAsync(new[] { userId }, numEntries))
            //    .OrderByDescending(item => item.Timestamp).Take(numEntries);
            //return entities.Select(UserHistoryEntity.CreateUserHistoryItem);
        }
    }
}
