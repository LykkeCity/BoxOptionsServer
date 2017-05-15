using System;

namespace BoxOptions.Core
{
    public class AssetQuote
    {
        // {"AssetPair":"BTCCHF","IsBuy":false,"Price":1620.427,"Timestamp":"2017-05-04T11:40:04.881Z"}
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
