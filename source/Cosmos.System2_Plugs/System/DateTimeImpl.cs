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

        public static string ToString(ref DateTime aThis)
        {
            // TODO: use current culture for string representation of DateTime
            return aThis.Year.ToString().PadLeft(4, '0') + "-" +
                   aThis.Month.ToString().PadLeft(2, '0') + "-" +
                   aThis.Day.ToString().PadLeft(2, '0') + " " +
                   aThis.Hour.ToString().PadLeft(2, '0') + ":" +
                   aThis.Minute.ToString().PadLeft(2, '0') + ":" +
                   aThis.Second.ToString().PadLeft(2, '0');
        }
    }
}
