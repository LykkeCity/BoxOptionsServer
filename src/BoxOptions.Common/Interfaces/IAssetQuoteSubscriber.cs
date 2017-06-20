using System;
using System.Threading.Tasks;

namespace BoxOptions.Common.Interfaces
{
    public interface IAssetQuoteSubscriber
    {
        event EventHandler<Core.Models.InstrumentPrice> MessageReceived;
        Task<bool> ReloadAssetConfiguration();
    }
}
