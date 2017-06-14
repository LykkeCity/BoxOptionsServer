using BoxOptions.Core.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BoxOptions.Common.Interfaces
{
    public interface IAssetDatabase
    {
        Task<LinkedList<BestBidAsk>> GetAssetHistory(DateTime dateFrom, DateTime dateTo, string assetPair);
        Task AddToAssetHistory(BestBidAsk bidask);
    }
}
