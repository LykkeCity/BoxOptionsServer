using BoxOptions.Common.Models;
using BoxOptions.Core.Interfaces;
using Microsoft.WindowsAzure.Storage.Table;
using System;

namespace BoxOptions.AzureRepositories.Entities
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

        public static GameBetEntity CreateEntity(IGameBetItem src)
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

        public static IGameBetItem CreateDto(GameBetEntity src)
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
}
