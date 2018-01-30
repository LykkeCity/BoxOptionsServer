using System;
using System.Collections.Generic;
using System.Text;

namespace BoxOptions.Services
{
    class CoefficientCache
    {
        private static readonly object CoeffCacheLock = new object();
        private Dictionary<string, CoefficientCacheItem> _coefCache;

        public CoefficientCache()
        {
            _coefCache = new Dictionary<string, CoefficientCacheItem>();
        }

        public string SetCache(string assetPair, string cache)
        {
            var validationResult = ValidateCache(cache);
            if (validationResult != "OK")
                return validationResult;
            lock (CoeffCacheLock)
            {
                if (!_coefCache.ContainsKey(assetPair))
                {
                    _coefCache.Add(assetPair, null);
                }
                _coefCache[assetPair] = new CoefficientCacheItem { Cache = cache, CacheDate = DateTime.UtcNow };
            }
            return "OK";
        }


        public string GetCache(string assetPair)
        {
            if (!_coefCache.ContainsKey(assetPair))
                throw new ArgumentException("AssetPair not in cache", "assetPair");
            lock (CoeffCacheLock)
            {
                return _coefCache[assetPair].Cache;
            }
        }


        private string ValidateCache(string cache)
        {
            Models.CoefModels.CoefRequestResult res = Models.CoefModels.CoefRequestResult.Parse(cache);
            bool AllEqualOne = true;

            // Test if there are negative 
            bool NegativeCoef = false;
            foreach (var block in res.CoefBlocks)
            {
                foreach (var coef in block.Coeffs)
                {
                    if (coef.HitCoeff != 1.0m || coef.MissCoeff != 1.0m)
                    {
                        AllEqualOne = false;
                    }

                    if (coef.HitCoeff < 0 || coef.MissCoeff < 0)
                    {
                        NegativeCoef = true;
                    }
                }
            }

            if (AllEqualOne == true) // All coefficients are 1.0 thow exception with information.
                return  "All coefficients are equal to 1.0";

            if (NegativeCoef == true) // Negative coefficiente
                return "Negative coefficients";

            return "OK";
        }
    }
}
