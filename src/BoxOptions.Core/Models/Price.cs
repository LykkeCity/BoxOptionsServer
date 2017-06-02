using System;

namespace BoxOptions.Core.Models
{
    public class Price
    {
        DateTime date;

        public double Bid { get; set; }
        public double Ask { get; set; }
        public DateTime Date { get => date; set => date = value; }
        /// <summary>
        /// Time in Java Milliseconds for calculations
        /// </summary>
        public long Time { get => date.GetJavaMillis(); }

        public double MidPrice()
        {
            return (Bid + Ask) / 2.0;
        }
        public virtual Price ClonePrice()
        {
            // all dates must be UTC
            //if (this.Date.Kind != DateTimeKind.Utc)
            //    throw new InvalidTimeZoneException();


            // DateTime with only 6 digits after seconds            
            string ticks = Date.Ticks.ToString();
            string trimed = ticks.Substring(0, ticks.Length - 1) + "0";
            long trimmedTicks = long.Parse(trimed);



            return new Price()
            {
                Ask = this.Ask,
                Bid = this.Bid,
                Date = new DateTime(trimmedTicks, DateTimeKind.Utc)
            };
        }
        public override string ToString()
        {
            return string.Format($"Price(time={this.Time}, bid={this.Bid}, ask={this.Ask})");
        }

    }
}
