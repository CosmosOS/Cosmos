using System;

using Cosmos.HAL;
using Cosmos.IL2CPU.Plugs;

namespace Cosmos.System.Plugs.System
{
    [Plug(Target = typeof(DateTime))]
    public static class DateTimeImpl
    {
        public static DateTime get_Now()
        {
            return new DateTime(RTC.Century * 100 + RTC.Year, RTC.Month, RTC.DayOfTheMonth, RTC.Hour, RTC.Minute, RTC.Second);
        }

        // TODO: get timezone
        public static DateTime get_UtcNow() => get_Now();

        public static long GetSystemTimeAsFileTime() => get_Now().Ticks;

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
