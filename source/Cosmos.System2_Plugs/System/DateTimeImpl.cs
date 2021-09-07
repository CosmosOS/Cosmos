using System;

using IL2CPU.API.Attribs;

using Cosmos.HAL;

namespace Cosmos.System_Plugs.System
{
    [Plug(typeof(DateTime))]
    public static class DateTimeImpl
    {
        public static DateTime Now =>
            new DateTime(
                RTC.Century * 100 + RTC.Year,
                RTC.Month,
                RTC.DayOfTheMonth,
                RTC.Hour,
                RTC.Minute,
                RTC.Second);

        // TODO: get timezone
        public static DateTime UtcNow => Now;

        public static long GetSystemTimeAsFileTime() => Now.Ticks;

        private static readonly string[] DaysOfWeekStrings = { "Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday"};
        private static readonly string[] MonthsStrings = { "January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "November", "December" };

        // The following code is assuming the INVARIANT culture for the strings conversions
        public static string ToLongDateString(ref DateTime aThis)
        {
            return DaysOfWeekStrings[(int)aThis.DayOfWeek] + ", " +
                   aThis.Day.ToString().PadLeft(2, '0') + " " +
                   MonthsStrings[aThis.Month - 1] + " " +
                   aThis.Year.ToString().PadLeft(4, '0');
        }

        public static string ToShortDateString(ref DateTime aThis)
        {
            return aThis.Month.ToString().PadLeft(2, '0') + '/' +
                   aThis.Day.ToString().PadLeft(2, '0') + '/' +
                   aThis.Year.ToString().PadLeft(4, '0');
        }

        public static string ToLongTimeString(ref DateTime aThis)
        {
            return aThis.Hour.ToString().PadLeft(2, '0') + ":" +
                   aThis.Minute.ToString().PadLeft(2, '0') + ":" +
                   aThis.Second.ToString().PadLeft(2, '0');
        }

        public static string ToShortTimeString(ref DateTime aThis)
        {
            return aThis.Hour.ToString().PadLeft(2, '0') + ":" +
                   aThis.Minute.ToString().PadLeft(2, '0');
        }

        public static string ToString(ref DateTime aThis)
        {
            
            return aThis.ToShortDateString() + " " + aThis.ToLongTimeString();
        }
    }
}
