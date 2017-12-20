using BoxOptions.Core.Interfaces;
using System;
using System.Threading.Tasks;

namespace BoxOptions.Common.Interfaces
{
    public interface IAssetQuoteSubscriber
    {
        event EventHandler<IInstrumentPrice> MessageReceived;
        Task<bool> ReloadAssetConfiguration();
    }
}
