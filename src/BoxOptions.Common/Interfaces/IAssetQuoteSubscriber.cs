using System;

namespace BoxOptions.Common.Interfaces
{
    public interface IAssetQuoteSubscriber
    {
        event EventHandler<Core.Models.InstrumentPrice> MessageReceived;
    }
}
