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
    public class GameRepository : IGameRepository
    {   
        private readonly INoSQLTableStorage<GameBetEntity> _betstorage;

        public GameRepository(INoSQLTableStorage<GameBetEntity> betstorage)
        {
            _betstorage = betstorage;
        }

        public async Task InsertGameBetAsync(IEnumerable<IGameBetItem> olapEntity)
        {

            var total = olapEntity.Select(GameBetEntity.CreateEntity);

            // Group by partition key
            var grouping = from e in total
                           group e by new { e.PartitionKey } into cms
                           select new { key = cms.Key, val = cms.ToList() };


            // Insert grouped baches 
            foreach (var item in grouping)
            {
                var list = item.val;
                do
                {
                    int bufferLen = 128;
                    if (list.Count < 128)
                        bufferLen = list.Count;
                    var buffer = list.Take(bufferLen);
                    await _betstorage.InsertOrReplaceBatchAsync(buffer);
                    list.RemoveRange(0, bufferLen);

                } while (list.Count > 0);
            }
            
        }

        public async Task<IEnumerable<IGameBetItem>> GetGameBetsByUser(string userId, DateTime dateFrom, DateTime dateTo)
        {
            DateTime startDate = dateFrom.Date;
            DateTime endDate = dateTo.Date.AddDays(1);
            DateTime currentDate = startDate;

            List<GameBetEntity> retval = new List<GameBetEntity>();
            do
            {
                string partitionKey = GameBetEntity.GetPartitionKey(userId, currentDate);
                var entities = (await _betstorage.GetDataAsync(new[] { partitionKey }, 100))
                .OrderByDescending(item => item.Timestamp);
                retval.AddRange(entities);

                currentDate = currentDate.AddDays(1);
            } while (currentDate < endDate);


            return retval.Select(GameBetEntity.CreateDto);

            //var entities = (await _betstorage.GetDataAsync(new[] { userId }, int.MaxValue,
            //    entity => entity.RowKey.StartsWith($"bet_") && entity.BetStatus == betState));
            //return entities.Select(GameBetEntity.CreateGameBetItem);
        }
        public async Task<IEnumerable<IGameBetItem>> GetGameBetsByUser(string userId, DateTime dateFrom, DateTime dateTo, int betState)
        {
            DateTime startDate = dateFrom.Date;
            DateTime endDate = dateTo.Date.AddDays(1);
            DateTime currentDate = startDate;

            List<GameBetEntity> retval = new List<GameBetEntity>();
            do
            {
                string partitionKey = GameBetEntity.GetPartitionKey(userId, currentDate);
                var entities = (await _betstorage.GetDataAsync(new[] { partitionKey }, 100,
                    entity => entity.BetStatus == betState))
                .OrderByDescending(item => item.Timestamp);
                retval.AddRange(entities);

                currentDate = currentDate.AddDays(1);
            } while (currentDate < endDate);


            return retval.Select(GameBetEntity.CreateDto);

            //var entities = (await _betstorage.GetDataAsync(new[] { userId }, int.MaxValue,
            //    entity => entity.RowKey.StartsWith($"bet_") && entity.BetStatus == betState));
            //return entities.Select(GameBetEntity.CreateGameBetItem);
        }
    }
}
