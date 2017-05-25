using BoxOptions.Common.Interfaces;
using System;

namespace BoxOptions.CoefficientCalculator
{
    public class Factory
    {
        public static ICoefficientCalculator CreateCalculator(IAssetQuoteSubscriber quoteFeed)
        {
            throw new NotImplementedException();
            //Calculator retval = new Calculator();
            //retval.Init(IAssetQuoteSubscriber)
        }


    }
}
