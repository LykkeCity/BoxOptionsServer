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
        public Price ClonePrice()
        {
            return new Price()
            {
                date = this.date,
                Ask = this.Ask,
                Bid = this.Bid
            };
        }
        public override string ToString()
        {
            return string.Format($"Price(time={this.Time}, bid={this.Bid}, ask={this.Ask})");
        }

    }
}
