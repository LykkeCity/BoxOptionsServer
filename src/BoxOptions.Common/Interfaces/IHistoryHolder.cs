using BoxOptions.Common.Models;
using System;
using System.Collections.Generic;

namespace BoxOptions.Common.Interfaces
{
    public interface IHistoryHolder
    {
        Price[] GetHistory(string asset);
        bool IsStarting { get; }

        event EventHandler InitializationFinished;
    }
}
