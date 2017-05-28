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
    public class GameEntity : TableEntity, IGameItem
    {
        public string UserId { get; set; }
        public string GameId { get; set; }
        public string AssetPair { get; set; }
        public DateTime CreationDate { get; set; }
        public DateTime FinishDate { get; set; }

        public static string GetPartitionKey(string userId)
        {
            return userId;
        }
        public static string GetRowKey(string gameId)
        {
            return $"game_{gameId}";
        }

        public static GameEntity Create(IGameItem src)
        {
            return new GameEntity
            {
                PartitionKey = GetPartitionKey(src.UserId),
                RowKey = GetRowKey(src.GameId),
                UserId = src.UserId,
                GameId = src.GameId,
                AssetPair = src.AssetPair,
                CreationDate = src.CreationDate,
                FinishDate = src.FinishDate
            };
        }

        public static GameItem CreateGameItem(GameEntity src)
        {
            if (src == null)
                return null;
            return new GameItem
            {
                UserId = src.UserId,
                GameId = src.GameId,
                AssetPair = src.AssetPair,
                CreationDate = src.CreationDate,
                FinishDate = src.FinishDate
            };
        }

    }

    public class GameBetEntity : TableEntity, IGameBetItem
    {
        public string UserId { get; set; }
        public string GameId { get; set; }        
        public string Box { get; set; }
        public DateTime Date { get; set; }
        public string BetAmount { get; set; }
        public string Parameters { get; set; }

        

        public static string GetPartitionKey(string userId)
        {
            return userId;
        }
        public static string GetRowKey(string gameId, DateTime date)
        {
            return $"game_{gameId}_bet_{date.ToString("yyyyMMddHHmmssfff")}";
        }

        public static GameBetEntity Create(IGameBetItem src)
        {
            return new GameBetEntity
            {
                PartitionKey = GetPartitionKey(src.UserId),
                RowKey = GetRowKey(src.GameId, src.Date),
                UserId = src.UserId,
                GameId = src.GameId,
                BetAmount = src.BetAmount,
                Box = src.Box,
                Date = src.Date,
                Parameters = src.Parameters
            };
        }

        public static GameBetItem CreateGameBetItem(GameBetEntity src)
        {
            if (src == null)
                return null;
            return new GameBetItem
            {
                UserId = src.UserId,
                GameId = src.GameId,
                BetAmount = src.BetAmount,
                Box = src.Box,
                Date = src.Date,
                Parameters = src.Parameters
                
            };
        }

    }

    public class GameRepository : IGameRepository
    {
        private readonly AzureTableStorage<GameEntity> _storage;
        private readonly AzureTableStorage<GameBetEntity> _betstorage;

        public GameRepository(AzureTableStorage<GameEntity> storage, AzureTableStorage<GameBetEntity> betstorage)
        {
            _storage = storage;
            _betstorage = betstorage;
        }
        public async Task InsertGameAsync(IGameItem olapEntity)
        {
            await _storage.InsertOrReplaceAsync(GameEntity.Create(olapEntity));
        }

        public async Task<GameItem> GetGame(string userId, string gameId)
        {
            var entities = (await _storage.GetDataAsync(new[] { userId }, int.MaxValue,
                entity => entity.RowKey == $"game_{gameId}"));
            return GameEntity.CreateGameItem(entities.FirstOrDefault());
        }

        public async Task<IEnumerable<GameItem>> GetGamesByUser(string userId, DateTime dateFrom, DateTime dateTo)
        {
            var entities = (await _storage.GetDataAsync(new[] { userId }, int.MaxValue,
                entity => entity.RowKey.StartsWith("game_")));
            return entities.Select(GameEntity.CreateGameItem);
        }

        public async Task InsertGameBetAsync(IGameBetItem olapEntity)
        {
            await _betstorage.InsertOrReplaceAsync(GameBetEntity.Create(olapEntity));
        }

        public async Task<IEnumerable<GameBetItem>> GetGameBetsByUser(string userId, string gameId)
        {
            var entities = (await _betstorage.GetDataAsync(new[] { userId }, int.MaxValue,
                entity => entity.RowKey.StartsWith($"game_{gameId}_bet_")));
            return entities.Select(GameBetEntity.CreateGameBetItem);
        }
    }
}
