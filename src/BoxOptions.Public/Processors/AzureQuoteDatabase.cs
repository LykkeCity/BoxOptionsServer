using BoxOptions.Common.Interfaces;
using BoxOptions.Core.Interfaces;
using BoxOptions.Core.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BoxOptions.Public.Processors
{
    public class AzureQuoteDatabase : IAssetDatabase, IDisposable
    {
        IAssetRepository assetRep;
        bool isDisposing = false;
        const int queueMaxSize = 100;
        Dictionary<string, Queue<IBestBidAsk>> assetCache;
        
        public AzureQuoteDatabase(IAssetRepository assetRep)
        {
            this.assetRep = assetRep;
            assetCache = new Dictionary<string, Queue<IBestBidAsk>>();
        }

        public Task AddToAssetHistory(IBestBidAsk bidask)
        {

            AddToHistory(bidask);
            return Task.FromResult(0);
        }
                
        private void AddToHistory(IBestBidAsk bidask)
        {

            if (!assetCache.ContainsKey(bidask.Asset))
                assetCache.Add(bidask.Asset, new Queue<IBestBidAsk>());

            assetCache[bidask.Asset].Enqueue(bidask);



            if (assetCache[bidask.Asset].Count >= queueMaxSize)
            {
                List<IBestBidAsk> buffer = assetCache[bidask.Asset].ToList();
                assetCache[bidask.Asset].Clear();
                InsertInAzure(buffer);
            }
        }

        private async Task InsertInAzure(List<IBestBidAsk> buffer)
        {
#if !DEBUG
                await assetRep.InsertManyAsync(buffer);
#endif    
        }
              
        public async Task<LinkedList<IBestBidAsk>> GetAssetHistory(DateTime dateFrom, DateTime dateTo, string assetPair)
        {
            var history = await assetRep.GetRange(dateFrom, dateTo, assetPair);
            var sorted = from h in history
                            orderby h.ReceiveDate
                            select h;
            return new LinkedList<IBestBidAsk>(sorted);
        }

        public void Dispose()
        {
            if (isDisposing)
                return;
            isDisposing = true;

            // Flush Asset Catch to Azure
            foreach (var key in assetCache.Keys)
            {
                if (assetCache[key].Count > 0)
                {
                    List<IBestBidAsk> buffer = assetCache[key].ToList();
                    assetCache[key].Clear();

                    InsertInAzure(buffer);
                }
            }
           


        }
    }
}
