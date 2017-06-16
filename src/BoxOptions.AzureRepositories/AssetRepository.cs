using AzureStorage;
using AzureStorage.Tables;
using BoxOptions.Core;
using BoxOptions.Core.Interfaces;
using BoxOptions.Core.Models;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace BoxOptions.AzureRepositories
{
    public class BestBidAskEntity : TableEntity, IBestBidAsk
    {
        public string Asset { get; set; }
        public double? BestAsk { get; set; }
        public double? BestBid { get; set; }
        public string Source { get; set; }

        DateTime IBestBidAsk.Timestamp { get; }

        public static string GetPartitionKey(IBestBidAsk src)
        {
            string key = string.Format("{0}_{1}", src.Asset, src.Timestamp.ToString("yyyyMMdd_HH"));
            return key;
        }
        public static string GetRowKey(IBestBidAsk src)
        {
            string key = src.Timestamp.Ticks.ToString();
            return key;
        }

        public static BestBidAskEntity Create(IBestBidAsk src)
        {
            return new BestBidAskEntity
            {
                PartitionKey = GetPartitionKey(src),
                RowKey = GetRowKey(src),
                Asset = src.Asset,
                BestAsk = src.BestAsk,
                BestBid = src.BestBid,
                Timestamp = src.Timestamp,
                Source = src.Source
            };
        }
        
        public static BestBidAsk CreateBestBidAsk(BestBidAskEntity src)
        {
            long ticks = long.Parse(src.RowKey);
            return new BestBidAsk
            {
                Asset = src.Asset,
                BestAsk = src.BestAsk,
                BestBid = src.BestBid,
                Timestamp = new DateTime(ticks, DateTimeKind.Utc),
                Source = src.Source
            };
        }
    }

    public class AssetRepository : IAssetRepository
    {
        const int maxbuffer = 100;
        private readonly AzureTableStorage<BestBidAskEntity> _storage;

        public AssetRepository(AzureTableStorage<BestBidAskEntity> storage)
        {
            _storage = storage;
        }

                
        public async Task InsertManyAsync(IEnumerable<IBestBidAsk> olapEntities)
        {
            var total = olapEntities.Select(BestBidAskEntity.Create);

            var grouping = from e in total
                           group e by new { e.PartitionKey } into cms
                           select new { key = cms.Key, val = cms.ToList() };


            foreach (var item in grouping)
            {
                var list = item.val;


                do
                {
                    int bufferLen = maxbuffer;
                    if (list.Count < maxbuffer)
                        bufferLen = list.Count;
                    var buffer = list.Take(bufferLen);
                    //Console.WriteLine("Inserting {0} records", bufferLen);
                    await _storage.InsertOrMergeBatchAsync(buffer);
                    list.RemoveRange(0, bufferLen);

                } while (list.Count > 0);
             
            }
        }

        public async Task<IEnumerable<BestBidAsk>> GetRange(DateTime dateFrom, DateTime dateTo, string assetPair)
        {
            DateTime currentDate = dateFrom.Date;
            List<BestBidAskEntity> retval = new List<BestBidAskEntity>();            
            do
            {
                string PartitionKey = string.Format("{0}_{1}", assetPair, currentDate.ToString("yyyyMMdd_HH"));
                
                var entities = (await _storage.GetDataAsync(new[] { PartitionKey }, int.MaxValue))
                .OrderByDescending(item => item.Timestamp);
                retval.AddRange(entities);
                // Next partition key (hours)
                currentDate = currentDate.AddHours(1);

            } while (currentDate < dateTo.Date.AddDays(1));
                        
            return retval.Select(BestBidAskEntity.CreateBestBidAsk);
        }


    }
}