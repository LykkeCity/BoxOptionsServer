using AzureStorage.Tables;
using BoxOptions.Core;
using System;
using System.Collections.Generic;
using System.Text;
using BoxOptions.Core.Interfaces;
using BoxOptions.Core.Models;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.Table;
using System.Linq;
using AzureStorage;

namespace BoxOptions.AzureRepositories
{
    
    public class BoxSizeEntity : TableEntity, IBoxSize
    {
        public string AssetPair { get; set; }
        public double TimeToFirstBox { get; set; }
        public double BoxHeight { get; set; }
        public double BoxWidth { get; set; }
        public int BoxesPerRow { get; set; }
        public bool SaveHistory { get; set; }
        public bool GameAllowed { get; set; }

        public static string GetPartitionKey()
        {
            return "BoxConfig";
        }

        public static BoxSizeEntity Create(IBoxSize src)
        {
            return new BoxSizeEntity
            {
                PartitionKey = GetPartitionKey(),
                RowKey = src.AssetPair,
                AssetPair = src.AssetPair,
                BoxesPerRow = src.BoxesPerRow,
                BoxHeight = src.BoxHeight,
                BoxWidth = src.BoxWidth,
                TimeToFirstBox = src.TimeToFirstBox,
                SaveHistory = src.SaveHistory,
                GameAllowed = src.GameAllowed
            };
        }

        public static BoxSize CreateBoxSizeItem(BoxSizeEntity src)
        {
            return new BoxSize
            {
                AssetPair = src.AssetPair,
                BoxesPerRow = src.BoxesPerRow,
                BoxHeight = src.BoxHeight,
                BoxWidth = src.BoxWidth,
                TimeToFirstBox = src.TimeToFirstBox,
                SaveHistory = src.SaveHistory,
                GameAllowed = src.GameAllowed
            };
        }
    }

    public class BoxConfigRepository : IBoxConfigRepository
    {
        private readonly INoSQLTableStorage<BoxSizeEntity> _storage;

        public BoxConfigRepository(INoSQLTableStorage<BoxSizeEntity> storage)
        {
            _storage = storage;
        }
       

        public async Task InsertAsync(IBoxSize olapEntity)
        {
            await _storage.InsertOrMergeAsync(BoxSizeEntity.Create(olapEntity));            
        }

        public async Task InsertManyAsync(IEnumerable<IBoxSize> olapEntities)
        {
            await _storage.InsertOrMergeBatchAsync(olapEntities.Select(BoxSizeEntity.Create));
        }

        public async  Task<BoxSize> GetAsset(string assetPair)
        {
            var asset = await _storage.GetDataAsync(new[] { "BoxConfig" }, 1,
                entity => entity.AssetPair == assetPair);
            return BoxSizeEntity.CreateBoxSizeItem(asset.FirstOrDefault());
        }

        public async Task<IEnumerable<BoxSize>> GetAll()
        {
            var asset = await _storage.GetDataAsync(new[] { "BoxConfig" }, int.MaxValue);
            return asset.Select(BoxSizeEntity.CreateBoxSizeItem);
        }
    }
}
