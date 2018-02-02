using BoxOptions.Core.Interfaces;

namespace BoxOptions.Public.Models
{
    public class BoxSizeModel : IBoxSize
    {
        public string AssetPair { get; set; }
        public double TimeToFirstBox { get; set; }
        public double BoxHeight { get; set; }
        public double BoxWidth { get; set; }
        public int BoxesPerRow { get; set; }
        public bool SaveHistory { get; set; }
        public bool GameAllowed { get; set; }
        public double ScaleK { get; set; }
        public double Volatility{ get; set; }
    }
}
