using BoxOptions.Common.Models;
using BoxOptions.Core.Interfaces;
using Microsoft.WindowsAzure.Storage.Table;

namespace BoxOptions.AzureRepositories.Entities
{
    public class BoxSizeEntity : TableEntity, IBoxSize
    {
        public string AssetPair { get; set; }
        public double TimeToFirstBox { get; set; }
        public double BoxHeight { get; set; }
        public double BoxWidth { get; set; }
        public int BoxesPerRow { get; set; }
        public bool SaveHistory { get; set; }
        public bool GameAllowed { get; set; }

        public static string GetPartitionKey()
        {
            return "BoxConfig";
        }

        public static BoxSizeEntity CreateEntity(IBoxSize src)
        {
            return new BoxSizeEntity
            {
                PartitionKey = GetPartitionKey(),
                RowKey = src.AssetPair,
                AssetPair = src.AssetPair,
                BoxesPerRow = src.BoxesPerRow,
                BoxHeight = src.BoxHeight,
                BoxWidth = src.BoxWidth,
                TimeToFirstBox = src.TimeToFirstBox,
                SaveHistory = src.SaveHistory,
                GameAllowed = src.GameAllowed
            };
        }

        public static IBoxSize CreateDto(BoxSizeEntity src)
        {
            return new BoxSize
            {
                AssetPair = src.AssetPair,
                BoxesPerRow = src.BoxesPerRow,
                BoxHeight = src.BoxHeight,
                BoxWidth = src.BoxWidth,
                TimeToFirstBox = src.TimeToFirstBox,
                SaveHistory = src.SaveHistory,
                GameAllowed = src.GameAllowed
            };
        }
    }
}
