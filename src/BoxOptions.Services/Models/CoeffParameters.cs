using System;
using System.Collections.Generic;
using System.Text;

namespace BoxOptions.Services.Models
{
    public class CoeffParameters
    {
        public int TimeToFirstOption { get; set; }
        public int OptionLen { get; set; }
        public double PriceSize { get; set; }
        public int NPriceIndex { get; set; }
        public int NTimeIndex { get; set; }
    }
}
