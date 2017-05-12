using Autofac;
using BoxOptions.Core;
using System;

namespace BoxOptions.Services
{
    public interface IAssetQuoteSubscriber : IStartable, IDisposable
    {
        event EventHandler<AssetPairBid> MessageReceived;
    }
}
