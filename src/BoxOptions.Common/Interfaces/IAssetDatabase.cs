using BoxOptions.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BoxOptions.Common.Interfaces
{
    public interface IAssetDatabase
    {
        Task<LinkedList<IBestBidAsk>> GetAssetHistory(DateTime dateFrom, DateTime dateTo, string assetPair);
        Task AddToAssetHistory(IBestBidAsk bidask);
    }
}
