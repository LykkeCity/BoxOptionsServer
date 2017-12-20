using BoxOptions.Common.Models;
using System.Collections.Generic;

namespace BoxOptions.CoefficientCalculator
{
    internal class PriceCache
    {
        private static object cacheLock = new object();

        private readonly Dictionary<string, List<Price>> cache = new Dictionary<string, List<Price>>();

        public void AddPrice(string instrument, Price price)
        {
            lock (cacheLock)
            {
                if (!cache.ContainsKey(instrument))
                    cache.Add(instrument, new List<Price>());
                cache[instrument].Add(price);
            }
        }

        public Price[] GetPrices(string instrument)
        {
            lock (cacheLock)
            {
                if (!cache.ContainsKey(instrument))
                    return new Price[0];
                var retval = cache[instrument].ToArray();
                cache.Remove(instrument);
                return retval;
            }
        }

    }
}
