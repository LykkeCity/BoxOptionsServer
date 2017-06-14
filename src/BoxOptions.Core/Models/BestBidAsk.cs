using System;
using System.Collections.Generic;
using System.Text;

namespace BoxOptions.Core.Models
{
    public class BestBidAsk : BoxOptions.Core.Interfaces.IBestBidAsk
    {
        public string Source { get; set; }
        public string Asset { get; set; }
        public DateTime Timestamp { get; set; }
        public double? BestAsk { get; set; }
        public double? BestBid { get; set; }

        public override string ToString()
        {
            return string.Format("{0}>[{1}]|{2}/{3}", Timestamp.ToString("yyyy-MM-dd HH:mm:ss.fff"), Asset, BestBid, BestAsk);
        }
    }
}
