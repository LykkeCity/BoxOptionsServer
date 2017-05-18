using System;
using System.Collections.Generic;
using System.Text;

namespace BoxOptions.Core.Interfaces
{
    public interface IAssetQuoteSubscriber
    {
        event EventHandler<InstrumentPrice> MessageReceived;
    }
}
