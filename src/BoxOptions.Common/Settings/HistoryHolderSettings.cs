using System;
using System.Collections.Generic;
using System.Text;

namespace BoxOptions.Common.Settings
{
    public class HistoryHolderSettings
    {
        public int NumberOfDaysInCache { get; set; }
        public string[] Assets { get; set; }
    }
}
