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

        public static string GetPartitionKey(string userId)
        {
            return userId;
        }
        public static string GetRowKey(string boxId, DateTime date)
        {
            return $"bet_{boxId}_{date.ToString("yyyyMMddHHmmssfff")}";
        }

        public static GameBetEntity Create(IGameBetItem src)
        {
            return new GameBetEntity
            {
                PartitionKey = GetPartitionKey(src.UserId),
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

        public async Task InsertGameBetAsync(IGameBetItem olapEntity)
        {
            await _betstorage.InsertOrReplaceAsync(GameBetEntity.Create(olapEntity));
        }

        public async Task<IEnumerable<GameBetItem>> GetGameBetsByUser(string userId, int betState)
        {
            var entities = (await _betstorage.GetDataAsync(new[] { userId }, int.MaxValue,
                entity => entity.RowKey.StartsWith($"bet_") && entity.BetStatus == betState));
            return entities.Select(GameBetEntity.CreateGameBetItem);
        }
    }
}
