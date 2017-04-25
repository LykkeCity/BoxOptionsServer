using System;

namespace BoxOptions.Core
{
    public class InstrumentBidAskPair
    {
        public string Instrument { get; set; }
        public DateTime Date { get; set; }
        public double Bid { get; set; }
        public double Ask { get; set; }
    }
}
