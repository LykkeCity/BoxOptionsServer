using BoxOptions.Core.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace BoxOptions.Core
{

    public interface IAssetRepository
    {        
        Task InsertManyAsync(IEnumerable<Interfaces.IBestBidAsk> olapEntities);
        Task<IEnumerable<BestBidAsk>> GetRange(DateTime dateFrom, DateTime dateTo, string assetPair);

    }
}
