using BoxOptions.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

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
