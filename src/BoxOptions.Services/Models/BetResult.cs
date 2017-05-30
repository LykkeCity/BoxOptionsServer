using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace BoxOptions.Services.Models
{
    public class BetResult
    {
        public string BoxId { get; set; }
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
