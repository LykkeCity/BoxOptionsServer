using System;

namespace BoxOptions.Core.Interfaces
{
    public interface IPlaceBetResult
    {
        DateTime BetTimeStamp { get;}
        string Status { get; }
    }
}
