using System;

namespace BoxOptions.Core.Interfaces
{
    public interface IAssetItem
    {
        string AssetPair { get;  }
        bool IsBuy { get; }
        double Price { get; }
        DateTime Date { get; }
    }
}
