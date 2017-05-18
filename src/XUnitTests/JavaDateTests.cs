
using BoxOptions.Core;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace XUnitTests
{
    public class JavaDateTests
    {
        [Fact]
        public void CreateDateFromJavaMilliseconds()
        {
            //ARRANGE
            // Test date 2017-05-17 12:30:00
            DateTime currentDate = new DateTime(2017, 05, 17, 12, 30, 00);

            // Milliseconds Since 1970-01-01
            // 17303 days, 12 hours, 30 min = 
            // 415272h + 12h + 30m = 
            // 24917040m + 30m =
            // 1495024200 sec = 
            // 1495024200000 millisec
            long MillisecondsSince_1970_01_01 = 1495024200000L;

            // ACT
            // Timespan to JavaEpochStart
            TimeSpan ts = (currentDate - BoxOptions.Core.Statics.JavaEpochStart);
            // Convert To long
            long ConvertedMilliseconds = (long)ts.TotalMilliseconds;
            // Instantiate new date.
            DateTime DotNetDate = DateTime.MinValue.AddTicks(currentDate.Ticks);

            // Teste Create date from Java Milliseconds
            DateTime JavaDate = BoxOptions.Core.Statics.CreateDateFromJavaMilliseconds(MillisecondsSince_1970_01_01);

            // Test GetJavaMillis function
            long javaMillis = DotNetDate.GetJavaMillis();


            //ASSERT
            // Compare milliseconds from calculated timespan
            Assert.Equal(MillisecondsSince_1970_01_01, ConvertedMilliseconds);
            // Compare JavaMillis
            Assert.Equal(MillisecondsSince_1970_01_01, javaMillis);
            // Compare dates
            Assert.Equal(DotNetDate, JavaDate);




        }
        [Fact]
        public void GetDayOfWeek()
        {
            //Arrange            
            // set date to 2017-05-17 12:30 (Wednesday)
            DateTime currentdate = new DateTime(2017, 05, 17, 12, 30, 00);

            //Act 
            DateTime WeekSunday = currentdate.GetWeekDay(DayOfWeek.Sunday);
            DateTime WeekMonday = currentdate.GetWeekDay(DayOfWeek.Monday);
            DateTime WeekTuesday = currentdate.GetWeekDay(DayOfWeek.Tuesday);
            DateTime WeekWednesday = currentdate.GetWeekDay(DayOfWeek.Wednesday);
            DateTime WeekThursday = currentdate.GetWeekDay(DayOfWeek.Thursday);
            DateTime WeekFriday = currentdate.GetWeekDay(DayOfWeek.Friday);
            DateTime WeekSaturday = currentdate.GetWeekDay(DayOfWeek.Saturday);

            //Assert
            Assert.Equal(new DateTime(2017, 05, 14, 0, 0, 0), WeekSunday);
            Assert.Equal(new DateTime(2017, 05, 15, 0, 0, 0), WeekMonday);
            Assert.Equal(new DateTime(2017, 05, 16, 0, 0, 0), WeekTuesday);
            Assert.Equal(new DateTime(2017, 05, 17, 0, 0, 0), WeekWednesday);
            Assert.Equal(new DateTime(2017, 05, 18, 0, 0, 0), WeekThursday);
            Assert.Equal(new DateTime(2017, 05, 19, 0, 0, 0), WeekFriday);
            Assert.Equal(new DateTime(2017, 05, 20, 0, 0, 0), WeekSaturday);

        }
    }
}
