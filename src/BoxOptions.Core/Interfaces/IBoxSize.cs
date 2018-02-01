namespace BoxOptions.Core.Interfaces
{
    public interface IBoxSize
    {
        /// <summary>
        /// Instrument
        /// </summary>        
        string AssetPair { get; }
        /// <summary>
        /// Seconds to first box
        /// </summary>
        double TimeToFirstBox { get; }
        /// <summary>
        /// Box Length in Seconds
        /// </summary>
        double BoxHeight { get; }
        /// <summary>
        /// Box price size 0.005% (0.00005)
        /// </summary>
        double BoxWidth { get; }
        /// <summary>
        /// Number of boxes in one row
        /// </summary>
        int BoxesPerRow { get; }
        /// <summary>
        /// Save asset bids in history
        /// </summary>
        bool SaveHistory { get; }
        /// <summary>
        /// Allowed to play in game
        /// </summary>
        bool GameAllowed { get; }
        /// <summary>
        /// Volatility Zoom Factor for box width calculation
        /// </summary>
        double VolatilityZoomFactor { get; }
    }
}
