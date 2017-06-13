using BoxOptions.Common.Interfaces;
using BoxOptions.Core;
using BoxOptions.Core.Interfaces;
using BoxOptions.Core.Models;
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
        int queueMaxSize = 100;
        Dictionary<string, Queue<AssetQuote>> assetCache;
        
        public AzureQuoteDatabase(IAssetRepository assetRep)
        {
            this.assetRep = assetRep;
            assetCache = new Dictionary<string, Queue<AssetQuote>>();
        }

        public Task AddToAssetHistory(AssetQuote quote)
        {

            AddToHistory(quote);
            return Task.FromResult(0);
        }

        
        private void AddToHistory(AssetQuote quote)
        {

            if (!assetCache.ContainsKey(quote.AssetPair))
                assetCache.Add(quote.AssetPair, new Queue<AssetQuote>());

            assetCache[quote.AssetPair].Enqueue(quote);



            if (assetCache[quote.AssetPair].Count >= queueMaxSize)
            {
                List<AssetQuote> buffer = assetCache[quote.AssetPair].ToList();
                assetCache[quote.AssetPair].Clear();
                InsertInAzure(buffer);
            }


         
        }

        private async void InsertInAzure(List<AssetQuote> buffer)
        {            
            try
            {            
                List<AssetItem> exportVector = (from q in buffer
                                                select new AssetItem
                                                {
                                                    AssetPair = q.AssetPair,
                                                    Date = q.Timestamp,
                                                    IsBuy = q.IsBuy,
                                                    Price = q.Price
                                                }).ToList();
                await assetRep.InsertManyAsync(exportVector);             
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }            
        }

        private async Task<IAssetItem> InsertInAzure(AssetQuote quote)
        {
            var res = await assetRep.InsertAsync(new AssetItem
            {
                AssetPair = quote.AssetPair,
                Date = quote.Timestamp,
                IsBuy = quote.IsBuy,
                Price = quote.Price
            });
            return res;
        }

        public async Task<LinkedList<AssetQuote>> GetAssetHistory(DateTime dateFrom, DateTime dateTo, string assetPair)
        {
            var history = await assetRep.GetRange(dateFrom, dateTo, assetPair); ;
            var converted = from h in history
                            orderby h.Date
                            select new AssetQuote()
                            {
                                AssetPair = h.AssetPair,
                                IsBuy = h.IsBuy,
                                Price = h.Price,
                                Timestamp = h.Date
                            };
            return new LinkedList<AssetQuote>(converted);
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
                    List<AssetQuote> buffer = assetCache[key].ToList();
                    assetCache[key].Clear();

                    InsertInAzure(buffer);
                }
            }
           


        }
    }
}
