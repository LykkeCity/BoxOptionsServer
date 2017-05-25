using BoxOptions.Core.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BoxOptions.Core.Interfaces
{
    public interface IBoxOptionsHistory
    {
        Task<LinkedList<AssetQuote>> GetAssetHistory(DateTime dateFrom, DateTime dateTo, string assetPair);
        Task AddToAssetHistory(AssetQuote quote);
    }
}
