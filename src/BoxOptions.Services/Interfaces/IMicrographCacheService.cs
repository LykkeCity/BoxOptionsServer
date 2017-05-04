﻿using System.Collections.Generic;
using BoxOptions.Core;

namespace BoxOptions.Services
{
    /// <summary>
    /// Connects to the rabbitmq 
    /// MicrographCacheService also connects to the same queue 
    /// (well actually they all create their own queues, but with the same routing key - mt.pricefeed, 
    /// so the prices routes to their queues in rabbit mq)
    /// </summary>
    public interface IMicrographCacheService
    {
        /// <summary>
        /// Get Current Graph Data
        /// </summary>
        /// <returns></returns>
        Dictionary<string, GraphBidAskPair[]> GetGraphData();
    }
}
