using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace BoxOptions.Services.Models
{
    public class BetResult
    {
        public BetResult(string boxId)
        {
            BoxId = boxId;
        }
        public string BoxId { get; private set; }
        public decimal MinPrice { get; set; }
        public decimal MaxPrice { get; set; }
        public double TimeToGraph { get; set; }
        public double TimeLength { get; set; }
        public decimal Coefficient { get; set; }
        public int BetState { get; set; }

        public Core.Models.InstrumentPrice PreviousPrice { get; set; }
        public Core.Models.InstrumentPrice CurrentPrice { get; set; }

        public DateTime? TimeToGraphStamp { get; set; }
        public DateTime? WinStamp { get; set; }
        public DateTime? FinishedStamp { get; set; }

        public DateTime Timestamp { get; set; }
        public decimal BetAmount { get; set; }        
        public bool IsWin { get; set; }

        public string ToJson()
        {
            string retval = JsonConvert.SerializeObject(this);
            return retval;

        }
    }
}
