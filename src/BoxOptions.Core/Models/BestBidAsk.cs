using System;

namespace BoxOptions.Core.Models
{
    public class BestBidAsk : Interfaces.IBestBidAsk
    {
        public string Source { get; set; }
        public string Asset { get; set; }        
        public double? BestAsk { get; set; }
        public double? BestBid { get; set; }
        public DateTime ReceiveDate { get; set; }
        public DateTime Timestamp { get; set; }

        public override string ToString()
        {
            return string.Format("{0}>[{1}]|{2}/{3}", ReceiveDate.ToString("yyyy-MM-dd HH:mm:ss.fff"), Asset, BestBid, BestAsk);
        }
    }
}
