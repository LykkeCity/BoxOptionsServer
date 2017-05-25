using System;

namespace BoxOptions.Core.Models
{
    public class InstrumentPrice
    {
        public string Instrument { get; set; }
        public DateTime Date { get; set; }
        public double Bid { get; set; }
        public double Ask { get; set; }

        public InstrumentPrice Clone()
        {
            return new InstrumentPrice()
            {
                Instrument = this.Instrument,
                Ask = this.Ask,
                Bid = this.Bid,
                Date = new DateTime(this.Date.Ticks)
            };
        }
    }
}
