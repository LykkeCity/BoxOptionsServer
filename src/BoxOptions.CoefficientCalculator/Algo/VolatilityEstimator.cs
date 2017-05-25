using BoxOptions.Core;
using BoxOptions.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BoxOptions.CoefficientCalculator.Algo
{
    public class VolatilityEstimator
    {
        public Runner runner;
        public List<double> activityDistribution;
        public List<Price> historicPrices; // need it only for initial estimation
        private long timeOfBar;
        public double periodsPerYear;
        public const long msInWeek = 604800000L;
        public int nBarStartTime; // in which activity bar is the current price
        public int nBarEndOfOpt; // in which activity bar is the end of the option
        public long movingWindow;
        public double prevVolat;
        public double volat;
        public bool initialized;

        public VolatilityEstimator(List<double> activityDistribution, List<Price> historicPrices, double delta, long movingWindow, double periodsPerYear)
        {
            runner = new Runner(delta, delta, -1);
            this.activityDistribution = activityDistribution;
            this.historicPrices = historicPrices;
            this.movingWindow = movingWindow;
            this.periodsPerYear = periodsPerYear;
            //TODO: Check Java Compat: 
            //timeOfBar = msInWeek / activityDistribution.size(); // divide number of ms per week to the number of bars
            timeOfBar = msInWeek / activityDistribution.Count; // divide number of ms per week to the number of bars
            initialized = false;
        }

        private double ComputeAnnualVolat(List<Price> historicPrices)
        {
            foreach (Price aPrice in historicPrices)
            {
                runner.Run(aPrice);
            }
            double sqrtVar = runner.ComputeSqrtVar();
            return Math.Sqrt((sqrtVar * periodsPerYear));
        }

        private double ComputeAverageActivity(Price currentPrice, long optEndsInMs)
        { // average activity for a given period of time

            long msFromMonday = MsFromMonday(currentPrice.Time);
            int firstBar = (int)(msFromMonday / timeOfBar);
            int lastBar = (int)((msFromMonday + optEndsInMs) / timeOfBar);
            nBarStartTime = firstBar;
            nBarEndOfOpt = lastBar;
            double sumActivity = 0.0;
            for (int iBar = firstBar; iBar <= lastBar; iBar++)
            {
                //sumActivity += activityDistribution.get(iBar % activityDistribution.size()); // will iterate the array if index is too big
                sumActivity += activityDistribution[iBar % activityDistribution.Count]; // will iterate the array if index is too big
            }
            return sumActivity / (double)(lastBar - firstBar + 1);
        }

        private long MsFromMonday(long currentTime)
        {
            // This methods was converted from JAVA, [currentTime] parameter should represent milliseconds since JAVA epoch:
            /*
            * Gets the milliseconds of the datetime instant from the Java epoch
            * of 1970-01-01T00:00:00Z.
            * 
            * @return the number of milliseconds since 1970-01-01T00:00:00Z
            * public long getMillis() {
            *   return iMillis;    }
            */
            // As such, a new method was created to construct a corresponding .NET datetime from the parameter received:
            // DateTime dateTime = new DateTime(currentTime);
            DateTime dateTime = Statics.CreateDateFromJavaMilliseconds(currentTime);

            // Created extension to Get the Week day of a given date:
            //DateTime mondayThisWeek = dateTime.withDayOfWeek(DateTimeConstants.MONDAY); // thus we get date of a Monday
            DateTime mondayThisWeek = dateTime.GetWeekDay(DayOfWeek.Monday); // thus we get date of a Monday

            /* This line is not needed since GetWeekDay returns date with time component set to zero.            
             * Since calculations are done in UTC timezone the timezone following assumption is not taken to account:
             * - [some time zones when Daylight Savings Time starts, there is no midnight because time jumps from 11:59 to 01:00*/
            // mondayThisWeek = mondayThisWeek.withTimeAtStartOfDay(); // to come to the very beginning of the Monday

            // Created extension GetJavaMillis
            //return currentTime - mondayThisWeek.getMillis();
            return currentTime - mondayThisWeek.GetJavaMillis();
        }
        /*
        /// <summary>
        /// .Net Implementation
        /// </summary>
        /// <param name="currentTime"></param>
        /// <returns></returns>
        private long MsFromMonday(DateTime currentTime)
        {                        
            DateTime mondayThisWeek = currentTime.GetWeekDay(DayOfWeek.Monday); // thus we get date of a Monday
            return (long)((currentTime - mondayThisWeek).TotalMilliseconds);
        }
        */

        public double Run(List<Price> newPrices, Price currentPrice, long optEndsInMs)
        {
            if (!initialized)
            {
                initialized = true;
                double annualVolat = ComputeAnnualVolat(historicPrices);
                double coeff = ComputeAverageActivity(currentPrice, optEndsInMs);
                volat = annualVolat * coeff;
                prevVolat = volat;
                return volat;

            }
            else
            {
                foreach (Price aPrice in newPrices)
                {
                    runner.Run(aPrice);
                }
                while ((runner.numberDC != 0) && (runner.timesDC.First() < currentPrice.Time - movingWindow))
                {
                    runner.timesDC.RemoveFirst();
                    runner.osLengths.RemoveFirst();
                    runner.numberDC -= 1;
                }
                double updatedSqrtVar = runner.ComputeSqrtVar();
                double annualVolat = Math.Sqrt((updatedSqrtVar * periodsPerYear));
                double coeff = ComputeAverageActivity(currentPrice, optEndsInMs);
                prevVolat = volat;
                volat = annualVolat * coeff;
                return volat;
            }
        }
    }
}
