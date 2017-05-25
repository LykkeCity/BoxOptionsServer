using BoxOptions.Core.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace BoxOptions.Core
{

    public interface IAssetRepository
    {
        Task<Interfaces.IAssetItem> InsertAsync(Interfaces.IAssetItem olapEntity);
        Task InsertManyAsync(IEnumerable<Interfaces.IAssetItem> olapEntities);
        Task<IEnumerable<AssetItem>> GetRange(DateTime dateFrom, DateTime dateTo, string assetPair);

    }
}
