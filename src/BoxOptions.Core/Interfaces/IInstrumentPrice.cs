using System;

namespace BoxOptions.Core.Interfaces
{
    public interface IInstrumentPrice
    {
        string Instrument { get; }
        DateTime ReceiveDate { get; }
        string Source { get; }
        double Bid { get; }
        double Ask { get; }
        DateTime Date { get; }
        long Time { get; }

        IInstrumentPrice ClonePrice();
    }
}
