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

    public class UserParameterEntity : TableEntity, IUserParameterItem
    {
        public string UserId { get; set; }
        public string AssetPair { get; set; }
        public int TimeToFirstOption { get; set; }
        public int OptionLen { get; set; }
        public double PriceSize { get; set; }
        public int NPriceIndex { get; set; }
        public int NTimeIndex { get; set; }

        public static string GetPartitionKey(string userId)
        {
            return userId;
        }
        public static string GetRowKey(string assetPair)
        {
            return $"userparam_{assetPair}";
        }

        public static UserParameterEntity Create(IUserParameterItem src)
        {
            return new UserParameterEntity
            {
                PartitionKey = GetPartitionKey(src.UserId),
                RowKey = GetRowKey(src.AssetPair),
                UserId = src.UserId,
                AssetPair = src.AssetPair,
                TimeToFirstOption = src.TimeToFirstOption,
                OptionLen = src.OptionLen,
                PriceSize = src.PriceSize,
                NPriceIndex = src.NPriceIndex,
                NTimeIndex = src.NTimeIndex
                
            };
        }

        public static UserParameterItem CreateUserParameterItem(UserParameterEntity src)
        {
            if (src == null)
                return null;
            return new UserParameterItem
            {
                UserId = src.UserId,
                AssetPair = src.AssetPair,
                TimeToFirstOption = src.TimeToFirstOption,
                OptionLen = src.OptionLen,
                PriceSize = src.PriceSize,
                NPriceIndex = src.NPriceIndex,
                NTimeIndex = src.NTimeIndex,
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

        

        public static string GetPartitionKey(string userId)
        {
            return userId;
        }
        public static string GetRowKey(DateTime histdate)
        {
            return $"userhist_{histdate.ToString("yyyy-MM-dd HH:mm:ss.fff")}";
        }

        public static UserHistoryEntity Create(IUserHistoryItem src)
        {
            return new UserHistoryEntity
            {
                PartitionKey = GetPartitionKey(src.UserId),
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
        private readonly AzureTableStorage<UserParameterEntity> _pstorage;
        private readonly AzureTableStorage<UserHistoryEntity> _hstorage;

        public UserRepository(AzureTableStorage<UserEntity> storage, AzureTableStorage<UserParameterEntity> pstorage, AzureTableStorage<UserHistoryEntity> hstorage)
        {
            _storage = storage;
            _pstorage = pstorage;
            _hstorage = hstorage;
        }

        public async Task InsertUserAsync(IUserItem olapEntity)
        {
            //await _storage.InsertAndGenerateRowKeyAsDateTimeAsync(UserEntity.Create(olapEntity), DateTime.UtcNow);
            await _storage.InsertOrReplaceAsync(UserEntity.Create(olapEntity));
        }

        public async Task<UserItem> GetUser(string userId)
        {
            var entities = (await _storage.GetDataAsync(new[] { userId }, int.MaxValue,
                entity => entity.RowKey == "User"));
            return UserEntity.CreateUserItem(entities.FirstOrDefault());
                
        }


        bool insertingParams = false;
        public async Task InsertManyParametersAsync(IEnumerable<IUserParameterItem> olapEntities)
        {
            if (insertingParams)
            {
                Console.WriteLine("{0}>Packet Lost: {1}", DateTime.UtcNow.ToString("HH:mm:ss"), olapEntities.Count());
                return;
            }
            insertingParams = true;

            var total = olapEntities.Select(UserParameterEntity.Create);

            await _pstorage.InsertOrMergeBatchAsync(total);
        
            insertingParams = false;
        }

        public async Task<IEnumerable<UserParameterItem>> GetUserParameters(string userId)
        {
            var entities = (await _pstorage.GetDataAsync(new[] { userId }, int.MaxValue,
                    entity => entity.RowKey.StartsWith("userparam_") ))
                .OrderByDescending(item => item.Timestamp);

            return entities.Select(UserParameterEntity.CreateUserParameterItem);
        }
                
        public async Task InsertHistoryAsync(IUserHistoryItem olapEntitiy)
        {            
            await _hstorage.InsertAsync(UserHistoryEntity.Create(olapEntitiy));
        }

        public async Task<IEnumerable<UserHistoryItem>> GetUserHistory(string userId, int numEntries)
        {
            var entities = (await _hstorage.GetDataAsync(new[] { userId }, numEntries,
                    entity => entity.RowKey.StartsWith("userhist_")))
                .OrderByDescending(item => item.Timestamp).Take(numEntries);

            return entities.Select(UserHistoryEntity.CreateUserHistoryItem);
        }
    }
}
