using AzureStorage;
using BoxOptions.AzureRepositories.Entities;
using BoxOptions.Core.Interfaces;
using BoxOptions.Core.Repositories;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BoxOptions.AzureRepositories
{
    public class UserRepository : IUserRepository
    {
        private readonly INoSQLTableStorage<UserEntity> _storage;        
        private readonly INoSQLTableStorage<UserHistoryEntity> _hstorage;

        public UserRepository(INoSQLTableStorage<UserEntity> storage, INoSQLTableStorage<UserHistoryEntity> hstorage)
        {
            _storage = storage;            
            _hstorage = hstorage;
        }

        public async Task InsertUserAsync(IEnumerable<IUserItem> olapEntity)
        {
            var total = olapEntity.Select(UserEntity.CreateEntity);
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
        public async Task<IUserItem> GetUser(string userId)
        {
            var entities = (await _storage.GetDataAsync(new[] { userId }, 1,
                entity => entity.RowKey == "User"));
            return UserEntity.CreateDto(entities.FirstOrDefault());
                
        }
        public async Task<IEnumerable<string>> GetUsers()
        {
            System.Collections.Concurrent.ConcurrentDictionary<string, byte> partitionKeys = new System.Collections.Concurrent.ConcurrentDictionary<string, byte>();
            await _storage.ExecuteAsync(new TableQuery<UserEntity>(), entity =>
            {
                foreach (var et in entity.Select(m => m.PartitionKey))
                    partitionKeys.TryAdd(et, 0);
            });
            return partitionKeys.Select(m => m.Key);
        }

        public async Task InsertHistoryAsync(IEnumerable<IUserHistoryItem> olapEntitiy)
        {
            var total = olapEntitiy.Select(UserHistoryEntity.CreateEntity);
            
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
        public async Task<IEnumerable<IUserHistoryItem>> GetUserHistory(string userId, DateTime dateFrom, DateTime dateTo)
        {

            DateTime startDate = dateFrom.Date;
            DateTime endDate = dateTo.Date.AddDays(1);
            DateTime currentDate = startDate;

            List<UserHistoryEntity> retval = new List<UserHistoryEntity>();
            do
            {
                string partitionKey = UserHistoryEntity.GetPartitionKey(userId, currentDate);
                var entities = (await _hstorage.GetDataAsync(new[] { partitionKey }, 100))
                .OrderByDescending(item => item.Timestamp);
                retval.AddRange(entities);

                currentDate = currentDate.AddDays(1);
            } while (currentDate < endDate);
                        

            return retval.Select(UserHistoryEntity.CreateDto);
        }

        
    }
}
