using System;
using System.Collections.Generic;
using System.Text;

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
    }
}
