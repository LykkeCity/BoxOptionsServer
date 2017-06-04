namespace BoxOptions.Core.Models
{
    public class BoxSize:Interfaces.IBoxSize
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
    }
}
