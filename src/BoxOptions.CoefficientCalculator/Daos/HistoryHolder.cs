using BoxOptions.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace BoxOptions.CoefficientCalculator.Daos
{
    class HistoryHolder
    {
        private Dictionary<string, LinkedList<Price>> historiesMap;

        public HistoryHolder()
        {
            historiesMap = new Dictionary<string, LinkedList<Price>>();
        }

        public void AddPrice(string instrument, Price price)
        {
            /*
                fun addPrice(instrument: String, price: Price) {
                    val prices = historiesMap.getOrPut(instrument) { LinkedList<Price>() }
                    if (prices.size > 0) {
                        val lastPrice = prices.last()
                        if (price.time > lastPrice.time) {
                            prices.add(price)
                        }
                    } else {
                        prices.add(price)
                    }
                }
            */

            // Add Instrument if doesn exist
            if (!historiesMap.ContainsKey(instrument))
            {
                historiesMap.Add(instrument, new LinkedList<Price>());
            }

            if (historiesMap[instrument].Count > 0)
            {
                var lastPrice = historiesMap[instrument].Last;
                if (price.Time > lastPrice.Value.Time)
                    historiesMap[instrument].AddLast(price);
            }
            else
            {
                historiesMap[instrument].AddLast(price);
            }
        }

        internal void BuildHistory(LinkedList<AssetQuote> assetQuote)
        {
            throw new NotImplementedException();
        }

        public List<Price> GetPrices(string instrument)
        {
            /*
                fun getPrices(instrument: String): List<Price>? {
                    return historiesMap[instrument]
                }
            */
            return new List<Price>(historiesMap[instrument]);

        }

        public void AddAllPrices(string instrument, List<Price> prices)
        {
            /*
                val prices = historiesMap.getOrPut(instrument.name) { LinkedList<Price>() }
                ticks.forEach { tick ->
                    prices.add(Price(tick.time, tick.bid, tick.ask))
                } 
            */

            // Add Instrument if doesn exist
            if (!historiesMap.ContainsKey(instrument))
            {
                historiesMap.Add(instrument, new LinkedList<Price>());
            }
            foreach (var price in prices)
            {
                historiesMap[instrument].AddLast(price);
            }
        }

        public override string ToString()
        {
            /*
                val builder = StringBuilder()
                historiesMap.entries.forEach {
                    builder.append("${it.key}: ${it.value.size}\n")
                }
                return builder.toString()
            */
            var builder = new StringBuilder();
            foreach (var item in historiesMap)
            {
                builder.Append($"{item.Key}: ${item.Value.Count}\n");
            }
            return builder.ToString();
        }
    }
}
