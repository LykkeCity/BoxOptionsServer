using BoxOptions.Core.Interfaces;

namespace BoxOptions.Common.Models
{
    public class BoxSize:IBoxSize
    {
        /// <summary>
        /// Instrument
        /// </summary>        
        public string AssetPair { get; set; }
        /// <summary>
        /// Seconds to first box
        /// </summary>
        public double TimeToFirstBox { get; set; }
        /// <summary>
        /// Box Length in Seconds
        /// </summary>
        public double BoxHeight { get; set; }
        /// <summary>
        /// Box price size 0.005% (0.00005)
        /// </summary>
        public double BoxWidth { get; set; }
        /// <summary>
        /// Number of boxes in one row
        /// </summary>
        public int BoxesPerRow { get; set; }
        /// <summary>
        /// Save asset bids in history
        /// </summary>
        public bool SaveHistory { get; set; }
        /// <summary>
        /// Allowed to play in game
        /// </summary>
        public bool GameAllowed { get; set; }
    }
}
