using BoxOptions.Common.Extensions;
using BoxOptions.Core.Interfaces;
using System;

namespace BoxOptions.Common.Models
{
    public class BestBidAsk : IBestBidAsk
    {
        public string Source { get; set; }
        public string Asset { get; set; }        
        public double? BestAsk { get; set; }
        public double? BestBid { get; set; }
        public DateTime ReceiveDate { get; set; }
        public DateTime Timestamp { get; set; }

        public override string ToString()
        {
            return string.Format("{0}>[{1}]|{2}/{3}", ReceiveDate.ToDateTimeString(), Asset, BestBid, BestAsk);
        }
    }
}
