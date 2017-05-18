using System;

namespace BoxOptions.Core
{
    public static class Statics
    {
            /*
           * Gets the milliseconds of the datetime instant from the Java epoch
           * of 1970-01-01T00:00:00Z.
           * 
           * @return the number of milliseconds since 1970-01-01T00:00:00Z
           * public long getMillis() {
           *   return iMillis;    }
           */

        /// <summary>
        /// Java Epoch Start 1970-01-01T00:00:00Z.
        /// </summary>
        public static DateTime JavaEpochStart { get => new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc); }

        /// <summary>
        /// Create a date from given JAVA milliseconds
        /// </summary>
        /// <param name="milliseconds"></param>
        /// <returns></returns>
        public static DateTime CreateDateFromJavaMilliseconds(long milliseconds)
        {
            return JavaEpochStart.AddMilliseconds(milliseconds);
        }


        /// <summary>
        /// Get milliseconds since JAVA epoch start: 1970-01-01T00:00:00Z.
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        public static long GetJavaMillis(this DateTime date)
        {
            return (long)(date - JavaEpochStart).TotalMilliseconds;

        }

        /// <summary>
        /// Get date of a given weekday from a known date only with date component filled, time componet is discarded.
        /// (2017-05-17_23:59:59).GetWeekDay(DayOfWeek.Monday) == 2017-05-15_00:00:00;
        /// </summary>
        /// <param name="date"></param>
        /// <param name="dayOfWeek"></param>
        /// <returns></returns>
        public static DateTime GetWeekDay(this DateTime date, DayOfWeek dayOfWeek)
        {
            // Take account that the week starts on Sunday:
            // Sunday = 0 and Saturday = 6
            int delta = dayOfWeek - date.DayOfWeek;
            DateTime result = date.Date.AddDays(delta);
            return result;

        }


        public const bool ASK = false; // Bid => IsBuy == true
        public static string[] AllowedAssets { get { return new[] { "EURUSD", "EURAUD", "EURCHF", "EURGBP", "EURJPY", "USDCHF", "BTCUSD" }; } }
    }
}
