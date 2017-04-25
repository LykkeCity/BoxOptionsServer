using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BoxOptions.Core
{
    public class AssetPair
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string BaseAssetId { get; set; }
        public string QuoteAssetId { get; set; }
        public int Accuracy { get; set; }
    }
}
