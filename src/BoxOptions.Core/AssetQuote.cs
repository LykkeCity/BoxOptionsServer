using System;

namespace BoxOptions.Core
{
    public class AssetQuote
    {        
        public string AssetPair { get; set; }
        public bool IsBuy { get; set; }
        public double Price { get; set; }
        public DateTime Timestamp { get; set; }

        public override string ToString()
        {
            return string.Format("[{0}]>{1} | {2}/{3}", AssetPair, Timestamp, IsBuy, Price);
        }

    }
}
