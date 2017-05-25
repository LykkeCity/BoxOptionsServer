using BoxOptions.Core;
using BoxOptions.Core.Models;
using System.Collections.Generic;
using System.Linq;

namespace BoxOptions.Common
{
    public class AssetBidProcessor
    {
        public static LinkedList<Price> CreateBidHistory(string assetPair, AssetQuote[] quotes)
        {

            var sortedList = from q in quotes
                             where q.AssetPair == assetPair
                             orderby q.Timestamp
                             select q;

            LinkedList<Price> result = new LinkedList<Price>();
            Price current = null;
            foreach (var item in sortedList)
            {
                if (current == null)
                    current = new Price() { Date = item.Timestamp };

                if (current.Ask == 0 && item.IsBuy == Statics.ASK)
                    current.Ask = item.Price;
                else if (current.Bid == 0 && item.IsBuy != Statics.ASK)
                    current.Bid = item.Price;

                if (current.Ask > 0 && current.Bid > 0)
                {
                    result.AddLast(current.ClonePrice());
                    current = null;
                }
            }

            return result;

            //List < Price > p = new 


        }

    }
}
