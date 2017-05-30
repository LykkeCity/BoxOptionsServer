using BoxOptions.Common.Interfaces;
using BoxOptions.Core.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BoxOptions.Services
{
    public class MockHistory : IAssetDatabase
    {     

        public Task<LinkedList<AssetQuote>> GetAssetHistory(DateTime dateFrom, DateTime dateTo, string assetPair)
        {
            LinkedList<AssetQuote> retval = new LinkedList<AssetQuote>();
            retval.AddLast(new AssetQuote() { Timestamp = DateTime.UtcNow.AddMinutes(-5), AssetPair = "EURUSD", IsBuy = false, Price = 1.09755d });
            retval.AddLast(new AssetQuote() { Timestamp = DateTime.UtcNow.AddMinutes(-4), AssetPair = "EURUSD", IsBuy = true, Price = 1.09754d });
            return  Task.FromResult(retval);
        }

        Task IAssetDatabase.AddToAssetHistory(AssetQuote quote)
        {
            throw new NotImplementedException();
        }
    }
}
