using System;

namespace BoxOptions.Core.Interfaces
{
    public interface ILogItem
    {
        string ClientId { get; }
        string EventCode { get; }
        string Message { get; }
        double AccountDelta { get; }

        DateTime Date { get; }
    }
}
