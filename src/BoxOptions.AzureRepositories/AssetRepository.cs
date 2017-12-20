using AzureStorage;
using BoxOptions.AzureRepositories.Entities;
using BoxOptions.Core.Interfaces;
using BoxOptions.Core.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BoxOptions.AzureRepositories
{
    public class AssetRepository : IAssetRepository
    {
        const int maxbuffer = 100;
        private readonly INoSQLTableStorage<BestBidAskEntity> _storage;

        public AssetRepository(INoSQLTableStorage<BestBidAskEntity> storage)
        {
            _storage = storage;
        }

                
        public async Task InsertManyAsync(IEnumerable<IBestBidAsk> olapEntities)
        {
            var total = olapEntities.Select(BestBidAskEntity.CreateEntity);

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

        public async Task<IEnumerable<IBestBidAsk>> GetRange(DateTime dateFrom, DateTime dateTo, string assetPair)
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
                        
            return retval.Select(BestBidAskEntity.CreateDto);
        }
    }
}