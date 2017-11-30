using AzureStorage;
using BoxOptions.AzureRepositories.Entities;
using BoxOptions.Core;
using BoxOptions.Core.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BoxOptions.AzureRepositories
{
    public class BoxConfigRepository : IBoxConfigRepository
    {
        private readonly INoSQLTableStorage<BoxSizeEntity> _storage;

        public BoxConfigRepository(INoSQLTableStorage<BoxSizeEntity> storage)
        {
            _storage = storage;
        }
       

        public async Task InsertAsync(IBoxSize olapEntity)
        {
            await _storage.InsertOrMergeAsync(BoxSizeEntity.CreateEntity(olapEntity));            
        }

        public async Task InsertManyAsync(IEnumerable<IBoxSize> olapEntities)
        {
            await _storage.InsertOrMergeBatchAsync(olapEntities.Select(BoxSizeEntity.CreateEntity));
        }

        public async  Task<IBoxSize> GetAsset(string assetPair)
        {
            var asset = await _storage.GetDataAsync(new[] { "BoxConfig" }, 1,
                entity => entity.AssetPair == assetPair);
            return BoxSizeEntity.CreateDto(asset.FirstOrDefault());
        }

        public async Task<IEnumerable<IBoxSize>> GetAll()
        {
            var asset = await _storage.GetDataAsync(new[] { "BoxConfig" }, int.MaxValue);
            return asset.Select(BoxSizeEntity.CreateDto);
        }
    }
}
