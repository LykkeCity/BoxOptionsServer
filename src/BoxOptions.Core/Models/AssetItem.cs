using System;

namespace BoxOptions.Core.Models
{
    public class AssetItem : Interfaces.IAssetItem
    {
        public string AssetPair { get; set; }
        public bool IsBuy { get; set; }
        public double Price { get; set; }
        public DateTime Date { get; set; }
        public string ServerTimestamp { get; set; }
    }
}
