using BoxOptions.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BoxOptions.Core
{
    public interface IAssetRepository
    {        
        Task InsertManyAsync(IEnumerable<IBestBidAsk> olapEntities);
        Task<IEnumerable<IBestBidAsk>> GetRange(DateTime dateFrom, DateTime dateTo, string assetPair);
    }
}
