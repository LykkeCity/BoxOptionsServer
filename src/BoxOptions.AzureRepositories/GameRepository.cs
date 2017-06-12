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
    public class GameBetEntity : TableEntity, IGameBetItem
    {
        public string UserId { get; set; }
        public string BoxId { get; set; }
        public string AssetPair { get; set; }
        public string Box { get; set; }
        public DateTime Date { get; set; }
        public string BetAmount { get; set; }
        public string Parameters { get; set; }
        public int BetStatus { get; set; }

        public static string GetPartitionKey(string userId, DateTime date)
        {
            return string.Format("{0}_{1}", userId, date.ToString("yyyyMMdd"));
        }
        public static string GetRowKey(string boxId, DateTime date)
        {
            return $"bet_{boxId}_{date.ToString("yyyyMMddHHmmssfff")}";
        }

        public static GameBetEntity Create(IGameBetItem src)
        {
            return new GameBetEntity
            {
                PartitionKey = GetPartitionKey(src.UserId, DateTime.UtcNow),
                RowKey = GetRowKey(src.BoxId, src.Date),
                UserId = src.UserId,
                BoxId = src.BoxId,
                AssetPair = src.AssetPair,
                BetAmount = src.BetAmount,
                Box = src.Box,
                Date = src.Date,
                Parameters = src.Parameters,
                BetStatus = src.BetStatus
            };
        }

        public static GameBetItem CreateGameBetItem(GameBetEntity src)
        {
            if (src == null)
                return null;
            return new GameBetItem
            {
                UserId = src.UserId,
                BoxId = src.BoxId,
                AssetPair = src.AssetPair,
                BetAmount = src.BetAmount,
                Box = src.Box,
                Date = src.Date,
                Parameters = src.Parameters,
                BetStatus = src.BetStatus
            };
        }

    }

    public class GameRepository : IGameRepository
    {
        
        private readonly AzureTableStorage<GameBetEntity> _betstorage;

        public GameRepository(AzureTableStorage<GameBetEntity> betstorage)
        {
            _betstorage = betstorage;
        }

        public async Task InsertGameBetAsync(IEnumerable<IGameBetItem> olapEntity)
        {

            var total = olapEntity.Select(GameBetEntity.Create);

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

        public async Task<IEnumerable<GameBetItem>> GetGameBetsByUser(string userId, DateTime dateFrom, DateTime dateTo)
        {
            DateTime startDate = dateFrom.Date;
            DateTime endDate = dateTo.Date.AddDays(1);
            DateTime currentDate = startDate;

            List<GameBetEntity> retval = new List<GameBetEntity>();
            do
            {
                string partitionKey = GameBetEntity.GetPartitionKey(userId, currentDate);
                var entities = (await _betstorage.GetDataAsync(new[] { partitionKey }, int.MaxValue))
                .OrderByDescending(item => item.Timestamp);
                retval.AddRange(entities);

                currentDate = currentDate.AddDays(1);
            } while (currentDate < endDate);


            return retval.Select(GameBetEntity.CreateGameBetItem);

            //var entities = (await _betstorage.GetDataAsync(new[] { userId }, int.MaxValue,
            //    entity => entity.RowKey.StartsWith($"bet_") && entity.BetStatus == betState));
            //return entities.Select(GameBetEntity.CreateGameBetItem);
        }
        public async Task<IEnumerable<GameBetItem>> GetGameBetsByUser(string userId, DateTime dateFrom, DateTime dateTo, int betState)
        {
            DateTime startDate = dateFrom.Date;
            DateTime endDate = dateTo.Date.AddDays(1);
            DateTime currentDate = startDate;

            List<GameBetEntity> retval = new List<GameBetEntity>();
            do
            {
                string partitionKey = GameBetEntity.GetPartitionKey(userId, currentDate);
                var entities = (await _betstorage.GetDataAsync(new[] { partitionKey }, int.MaxValue,
                    entity => entity.BetStatus == betState))
                .OrderByDescending(item => item.Timestamp);
                retval.AddRange(entities);

                currentDate = currentDate.AddDays(1);
            } while (currentDate < endDate);


            return retval.Select(GameBetEntity.CreateGameBetItem);

            //var entities = (await _betstorage.GetDataAsync(new[] { userId }, int.MaxValue,
            //    entity => entity.RowKey.StartsWith($"bet_") && entity.BetStatus == betState));
            //return entities.Select(GameBetEntity.CreateGameBetItem);
        }
    }
}
