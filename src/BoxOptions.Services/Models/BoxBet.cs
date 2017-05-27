using System;

namespace BoxOptions.Services.Models
{
    class BoxBet
    {
        public decimal BetAmount { get; set; }
        public DateTime Timestamp { get; set; }
        public Box Box { get; set; }


        public override string ToString()
        {
            return string.Format("{0} > {1:f4}", this.Timestamp.ToString("yyyy-MM-dd HH:mm:ss.fff"), BetAmount);
        }
    }
}
