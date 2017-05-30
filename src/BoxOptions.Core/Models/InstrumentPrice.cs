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
            // all dates must be UTC
            if (this.Date.Kind != DateTimeKind.Utc)
                throw new InvalidTimeZoneException();


            // DateTime with only 6 digits after seconds            
            string ticks = Date.Ticks.ToString();
            string trimed = ticks.Substring(0, ticks.Length - 1) + "0";
            long trimmedTicks = long.Parse(trimed);



            return new InstrumentPrice()
            {
                Instrument = this.Instrument,
                Ask = this.Ask,
                Bid = this.Bid,
                Date = new DateTime(trimmedTicks, DateTimeKind.Utc)
            };
        }
    }
}
