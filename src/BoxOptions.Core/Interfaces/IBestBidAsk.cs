using System;

namespace BoxOptions.Core.Interfaces
{
    public interface IBestBidAsk
    {
        string Asset { get; }
        double? BestAsk { get; }
        double? BestBid { get; }
        string Source { get; }
        DateTime Timestamp { get; }
        DateTime ReceiveDate { get; }
    }
}