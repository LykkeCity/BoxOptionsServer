using BoxOptions.Core.Interfaces;
using System;

namespace BoxOptions.Core.Models
{
    public class GameItem : IGameItem
    {
        public string UserId { get; set; }
        public string GameId { get; set; }
        public string AssetPair { get; set; }
        public DateTime CreationDate { get; set; }
        public DateTime FinishDate { get; set; }
    }
}
