using BoxOptions.Core;
using System;

namespace BoxOptions.CoefficientCalculator
{
    internal static class Extensions
    {
        public static bool CheckIfNowIsWeekend(this long currentTime, long timeWeekendStarts, long timeWeekendEnds)
        {
            long currentTimeFromMonday = FindTimeFromLastMonday(currentTime);
            bool nowIsWeekend = false;
            if (currentTimeFromMonday >= timeWeekendStarts && currentTimeFromMonday <= timeWeekendEnds)
            {
                nowIsWeekend = true;
            }
            return nowIsWeekend;
        }
        public static long FindTimeFromLastMonday(long currentTime)
        {
            DateTime dateTime = new DateTime(currentTime);
            DateTime mondayThisWeek = dateTime.GetWeekDay(DayOfWeek.Monday); // thus we get date of a Monday
            //Not needed
            //mondayThisWeek = mondayThisWeek.withTimeAtStartOfDay(); // to come to the very beginning of the Monday
            return currentTime - mondayThisWeek.GetJavaMillis();
        }

    }
}
