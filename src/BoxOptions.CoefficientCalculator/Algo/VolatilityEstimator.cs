using BoxOptions.Core;
using BoxOptions.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BoxOptions.CoefficientCalculator.Algo
{
    internal class VolatilityEstimator
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
        public long timeWeekendStarts, timeWeekendEnds;
        public bool hasWeekend;
        public bool addedWeekend; // shows whether we've already added a weekend to the list of IE or not

        public VolatilityEstimator(List<double> activityDistribution, List<Price> historicPrices, double delta, long movingWindow, double periodsPerYear, bool hasWeekend)
        {
            runner = new Runner(delta, delta, -1, movingWindow);
            this.activityDistribution = activityDistribution;
            this.historicPrices = historicPrices;
            this.movingWindow = movingWindow;
            this.periodsPerYear = periodsPerYear;
            this.hasWeekend = hasWeekend;
            timeOfBar = msInWeek / activityDistribution.Count(); // divide number of ms per week to the number of bars
            initialized = false;
            if (hasWeekend)
            {
                timeWeekendStarts = 421200000L;
                timeWeekendEnds = 586800000L;
            }
            else
            {
                timeWeekendStarts = timeWeekendEnds = -1L;
            }
            addedWeekend = false;
            //Not needed in .NET, all datetime variables are set as UTC 
            //DateTimeZone.setDefault(DateTimeZone.UTC); // it is an important field: without this the algorithm will
                                                       // interpret time like my local time. https://stackoverflow.com/questions/9397715/defaulting-date-time-zone-to-utc-for-jodatimes-datetime
                                                       
        }
        private double ComputeAverageFutureActivity(Price currentPrice, long optEndsInMs)
        { // average activity for a given period of time

            long msFromMonday = MsFromMonday(currentPrice.Time);
            int firstBar = (int)(msFromMonday / timeOfBar);
            int lastBar = (int)((msFromMonday + optEndsInMs) / timeOfBar);
            nBarStartTime = firstBar;
            nBarEndOfOpt = lastBar;
            double sumActivity = 0.0;
            for (int iBar = firstBar; iBar <= lastBar; iBar++)
            {
                sumActivity += activityDistribution[iBar % activityDistribution.Count]; // will iterate the array if index is too big
            }
            return sumActivity / (lastBar - firstBar + 1); // finds average activity till the end of the box
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

        private void Initialize(List<Price> historicalPrices)
        {
            Price previousPrice = historicalPrices[0];
            long weekendLength = timeWeekendEnds - timeWeekendStarts;
            foreach (var currentPrice in historicalPrices)
            {   
                if (currentPrice.Time - previousPrice.Time > weekendLength / 2)
                { // to catch a weekend gap. At least half, to be sure.
                    runner.AddTimeToIEs(timeWeekendEnds - timeWeekendStarts);
                }
                runner.Run(currentPrice);
                previousPrice = currentPrice.ClonePrice();
            }
        }


        public double Run(List<Price> newPrices, Price currentPrice, long optEndsInMs)
        {            
            if (!initialized)
            {
                initialized = true;
                Initialize(historicPrices); // just initializes all runners and finds historical IEs
                prevVolat = volat = 0.0; // this part prevents wrong payouts on weekends
                return volat;

            }
            else
            {
                //if (Tools.checkIfNowIsWeekend(timeWeekendStarts, timeWeekendEnds, currentPrice.Time))
                if (currentPrice.Time.CheckIfNowIsWeekend(timeWeekendStarts, timeWeekendEnds))
                {
                    if (!addedWeekend)
                    {
                        runner.AddTimeToIEs(timeWeekendEnds - timeWeekendStarts);
                        addedWeekend = true;
                    }
                    prevVolat = volat;
                    volat = 0.0;
                }
                else
                {
                    foreach (var aPrice in newPrices)
                    {
                        runner.Run(aPrice);
                    }
                    double updatedSqrtVar = runner.ComputeTotalSqrtVar();
                    double annualVolat = Math.Sqrt((updatedSqrtVar * periodsPerYear));
                    double coeff = ComputeAverageFutureActivity(currentPrice, optEndsInMs);
                    prevVolat = volat;
                    volat = annualVolat * coeff;
                    addedWeekend = false;
                }
                return volat;
            }
        }
    }
}
