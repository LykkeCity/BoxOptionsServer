using System;
using System.Collections.Generic;
using System.Text;

namespace BoxOptions.Services.Models
{
    public class BetResult
    {
        public string UserId { get; set; }
        public string GameId { get; set; }
        public string BoxId { get; set; }
        public DateTime Timestamp { get; set; }
        public decimal BetAmount { get; set; }
        public Box BoxInfo { get; set; }

    }
}
