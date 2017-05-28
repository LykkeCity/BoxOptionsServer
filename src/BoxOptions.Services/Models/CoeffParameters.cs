using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace BoxOptions.Services.Models
{
    public class CoeffParameters
    {
        public string AssetPair { get; set; }
        public int TimeToFirstOption { get; set; }
        public int OptionLen { get; set; }
        public double PriceSize { get; set; }
        public int NPriceIndex { get; set; }
        public int NTimeIndex { get; set; }

        public static CoeffParameters FromJson(string json)
        {
            CoeffParameters retval = JsonConvert.DeserializeObject<CoeffParameters>(json);
            return retval;
        }
    }
}
