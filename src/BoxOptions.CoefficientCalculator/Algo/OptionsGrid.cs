using BoxOptions.Common.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace BoxOptions.CoefficientCalculator.Algo
{
    internal class OptionsGrid
    {
        private const long MS_PER_YEAR = 31536000000L;

        long timeToFirstOption;
        long optionLen;
        double priceSize;
        int nPriceIndexes;
        int nTimeIndexes;
        double marginHit;
        double marginMiss;
        double maxPayoutCoeff;
        double bookingFee;
        public BoxOption[][] optionsGrid;
        VolatilityEstimator[] volatilityEstimators;
        bool hasWeekend;

        public OptionsGrid(long timeToFirstOption, long optionLen, double priceSize, int nPriceIndexes, int nTimeIndexes, double marginHit, double marginMiss, double maxPayoutCoeff, double bookingFee, bool hasWeekend)
        {

            this.timeToFirstOption = timeToFirstOption;
            this.optionLen = optionLen;
            this.priceSize = priceSize;
            this.nPriceIndexes = nPriceIndexes;
            this.nTimeIndexes = nTimeIndexes;
            this.marginHit = marginHit;
            this.marginMiss = marginMiss;
            this.maxPayoutCoeff = maxPayoutCoeff;
            this.bookingFee = bookingFee;
            this.hasWeekend = hasWeekend;
            
            // jagged array 2nd  array must initialized locally
            //optionsGrid = new BoxOption[nTimeIndexes][nPriceIndexes];
            optionsGrid = new BoxOption[nTimeIndexes][];
            volatilityEstimators = new VolatilityEstimator[nTimeIndexes];
        }

        public void InitiateGrid(List<Double> activityDistribution, List<Price> historicPrices, double delta, long movingWindow, Price price)
        {

            double minRelatBottomStrike = -(priceSize * nPriceIndexes / 2.0);
            double minRelatUpperStrike = minRelatBottomStrike + priceSize;
            for (int i = 0; i < nTimeIndexes; i++)
            {
                long optStartsInMs = timeToFirstOption + i * optionLen;
                long optEndsInMs = optStartsInMs + optionLen;
                volatilityEstimators[i] = new VolatilityEstimator(activityDistribution, historicPrices, delta, movingWindow, (MS_PER_YEAR / (double)movingWindow), hasWeekend);
                //volatilityEstimators[i].run(new ArrayList<>(), price, optEndsInMs);
                volatilityEstimators[i].Run(new List<Price>(), price, optEndsInMs);
                // initialize jagged array
                optionsGrid[i] = new BoxOption[nPriceIndexes];
                for (int j = 0; j < nPriceIndexes; j++)
                {
                    optionsGrid[i][j] = new BoxOption(optStartsInMs, optEndsInMs, minRelatUpperStrike + j * priceSize, minRelatBottomStrike + j * priceSize);
                  //BoxPricing boxPricing = new BoxPricing(price.getTime() + optStartsInMs, price.getTime() + optEndsInMs,  optionsGrid[i][j].relatUpStrike + price.midPrice(), optionsGrid[i][j].relatBotStrike + price.midPrice(), price, marginHit, marginMiss, maxPayoutCoeff, bookingFee);
                    BoxPricing boxPricing = new BoxPricing(price.Time + optStartsInMs,      price.Time + optEndsInMs,       optionsGrid[i][j].RelatUpStrike + price.MidPrice(), optionsGrid[i][j].RelatBotStrike + price.MidPrice(), price, marginHit, marginMiss, maxPayoutCoeff, bookingFee);
                    optionsGrid[i][j].SetCoefficients(boxPricing.GetCoefficients(0, volatilityEstimators[i].volat));
                }
            }
        }

        public void UpdateCoefficients(List<Price> newPrices, Price price)
        {
            for (int i = 0; i < nTimeIndexes; i++)
            {                
                double volatility = volatilityEstimators[i].Run(newPrices, price, optionsGrid[i][0].StartsInMS + optionsGrid[i][0].LenInMS);
                if (volatility != volatilityEstimators[i].prevVolat)
                {
                    for (int j = 0; j < nPriceIndexes; j++)
                    {
                        BoxPricing boxPricing = new BoxPricing(price.Time + optionsGrid[i][j].StartsInMS, price.Time + optionsGrid[i][j].StartsInMS + optionsGrid[i][j].LenInMS, optionsGrid[i][j].RelatUpStrike + price.MidPrice(), optionsGrid[i][j].RelatBotStrike + price.MidPrice(), price, marginHit, marginMiss, maxPayoutCoeff, bookingFee);
                        optionsGrid[i][j].SetCoefficients(boxPricing.GetCoefficients(0, volatilityEstimators[i].volat));
                    }
                }
            }
        }
    }
}
