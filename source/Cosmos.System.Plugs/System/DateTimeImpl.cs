using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cosmos.HAL;
using Cosmos.IL2CPU.Plugs;

namespace Cosmos.System.Plugs.System
{
    [Plug(Target = typeof(global::System.DateTime))]
    public class DateTimeImpl
    {
        public static DateTime Now
        {
            get
            {
                return new DateTime(RTC.Century * 100 + RTC.Year, RTC.Month, RTC.DayOfTheMonth, RTC.Hour, RTC.Minute, RTC.Second);
            }
        }
        public static DateTime Today
        {
            get
            {
                return new DateTime(RTC.Century * 100 + RTC.Year, RTC.Month, RTC.DayOfTheMonth, 0, 0, 0);
            }
        }

        public static string ToString(ref DateTime aThis)
        {
            return "dt";
        }
    }
}
