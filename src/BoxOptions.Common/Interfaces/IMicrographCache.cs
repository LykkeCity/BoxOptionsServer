using BoxOptions.Common.Models;
using System.Collections.Generic;

namespace BoxOptions.Common.Interfaces
{
    /// <summary>
    /// Connects to the rabbitmq 
    /// MicrographCacheService also connects to the same queue 
    /// (well actually they all create their own queues, but with the same routing key - mt.pricefeed, 
    /// so the prices routes to their queues in rabbit mq)
    /// </summary>
    public interface IMicrographCache
    {
        /// <summary>
        /// Get Current Graph Data
        /// </summary>
        /// <returns></returns>
        Dictionary<string, Price[]> GetGraphData();
    }
}
