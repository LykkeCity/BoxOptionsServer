using System;

namespace BoxOptions.Core.Interfaces
{
    public interface IAssetQuoteSubscriber
    {
        event EventHandler<Models.InstrumentPrice> MessageReceived;
    }
}
