/*
Copyright (c) 2012-2013, dewitcher Team
All rights reserved.

Redistribution and use in source and binary forms, with or without modification,
are permitted provided that the following conditions are met:

* Redistributions of source code must retain the above copyright notice
   this list of conditions and the following disclaimer.

* Redistributions in binary form must reproduce the above copyright notice,
  this list of conditions and the following disclaimer in the documentation
  and/or other materials provided with the distribution.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR
IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND
FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR
CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL
DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE,
DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER
IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF
THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
*/

namespace DuNodes.Kernel.Base
{
    public static class RTC
    {
        /// <summary>
        /// NOT RECOMMENDED! Waits for a given amount of ticks. It depends on the CPU speed.
        /// </summary>
        /// <param name="ticks">Amount</param>
        public static void SleepTicks(int ticks)
        {
            for (int i = 0; i < ticks; i++) { ;} return;
        }
        
        public static class Now
        {
            /// <summary>
            /// Returns the hour
            /// </summary>
            public static byte Hour { get { return Cosmos.Hardware.RTC.Hour; } }
            /// <summary>
            /// Returns the hour as string in format hh
            /// </summary>
            public static string HourString { get { return Hour.TryAppend(); } }

            /// <summary>
            /// Returns the minute
            /// </summary>
            public static byte Minute { get { return Cosmos.Hardware.RTC.Minute; } }
            /// <summary>
            /// Returns the minute as string in format mm
            /// </summary>
            public static string MinuteString { get { return Minute.TryAppend(); } }

            /// <summary>
            /// Returns the second
            /// </summary>
            public static byte Second { get { return Cosmos.Hardware.RTC.Second; } }
            /// /// <summary>
            /// Returns the second as string in format ss
            /// </summary>
            public static string SecondString { get { return Second.TryAppend(); } }

            /// <summary>
            /// Returns the century
            /// </summary>
            public static byte Century { get { return (byte)(Cosmos.Hardware.RTC.Century); } }
            /// <summary>
            /// Returns the century as string in format xx ( x = any number )
            /// </summary>
            public static string CenturyString { get { return Century.TryAppend(); } }

            /// <summary>
            /// Returns the year (e.g. 2012)
            /// </summary>
            public static int Year { get { return int.Parse(((Century).ToString() + Cosmos.Hardware.RTC.Year.ToString())); } }
            /// <summary>
            /// Returns the year as string in format xxxx ( x = any number )
            /// </summary>
            public static string YearString { get { return (CenturyString + Cosmos.Hardware.RTC.Year.TryAppend()); } }

            /// <summary>
            /// Returns the day of the month
            /// </summary>
            public static byte DayOfTheMonth { get { return Cosmos.Hardware.RTC.DayOfTheMonth; } }
            /// <summary>
            /// Returns the day of the month in format xx ( x = any number )
            /// </summary>
            public static string DayOfTheMonthString { get { return DayOfTheMonth.TryAppend(); } }

            /// <summary>
            /// Returns the day of the year
            /// </summary>
            public static byte DayOfTheYear { get { return ((byte)(DayOfTheMonth * Month)); } }
            /// <summary>
            /// Returns the day of the year in format xxx ( x = any number )
            /// </summary>
            public static string DayOfTheYearString { get { return DayOfTheYear.TryAppendYear(); } }

            /// <summary>
            /// Returns the day of the week
            /// </summary>
            public static byte DayOfTheWeek { get { return Cosmos.Hardware.RTC.DayOfTheWeek; } }
            /// <summary>
            /// Returns the day of the week in format xx ( x = any number )
            /// </summary>
            public static string DayOfTheWeekString { get { return DayOfTheWeek.TryAppend(); } }

            /// <summary>
            /// Returns the month
            /// </summary>
            public static byte Month { get { return Cosmos.Hardware.RTC.Month; } }
            /// <summary>
            /// Returns the month in format xx ( x = any number )
            /// </summary>
            public static string MonthString { get { return Month.TryAppend(); } }

            /// <summary>
            /// DateFormat
            /// </summary>
            public enum DateFormat : byte { YYYY_MM_DD, DD_MM_YYYY, MM_DD_YYYY, YYYY_DD_MM }
            /// <summary>
            /// Returns the date in the specified date format
            /// </summary>
            /// <param name="format">Date Format</param>
            /// <returns>the date in the specified date format</returns>
            public static string GetDate(DateFormat format, char separator = '.')
            {
                if (format == DateFormat.DD_MM_YYYY)
                    return (DayOfTheMonthString + separator + MonthString + separator + YearString);
                else if (format == DateFormat.YYYY_MM_DD)
                    return (YearString + separator + MonthString + separator + DayOfTheMonthString);
                else if (format == DateFormat.YYYY_DD_MM)
                    return (YearString + "." + DayOfTheMonthString + separator + MonthString);
                else if (format == DateFormat.MM_DD_YYYY)
                    return (MonthString + "." + DayOfTheMonthString + separator + YearString);
                else
                    return "ERROR";
            }

            /// <summary>
            /// TimeFormat
            /// </summary>
            public enum TimeFormat : byte { hh_mm, hh_mm_ss, mm_ss }
            /// <summary>
            /// Returns the time in the specified time format
            /// </summary>
            /// <param name="format">Time Format</param>
            /// <returns>the time in the specified time format</returns>
            public static string GetTime(TimeFormat format)
            {
                if (format == TimeFormat.hh_mm)
                    return (HourString + ":" + MinuteString);
                else if (format == TimeFormat.hh_mm_ss)
                    return (HourString + ":" + MinuteString + ":" + SecondString);
                else if (format == TimeFormat.mm_ss)
                    return (MinuteString + ":" + SecondString);
                else
                    return "ERROR";
            }
        }
        /// <summary>
        /// Function that tries to set the numbers length to 2
        /// </summary>
        /// <param name="value">byte</param>
        /// <returns>new value as string</returns>
        internal static string TryAppend(this byte value)
        {
            string val = value.ToString();
            if (val.Length < 2)
                return ("0" + val);
            else
                return val;
        }
        /// <summary>
        /// Function that tries to set the numbers length to 3
        /// </summary>
        /// <param name="value">byte</param>
        /// <returns>new value as string</returns>
        internal static string TryAppendYear(this byte value)
        {
            string val = value.ToString();
            if (val.Length < 2)
                return ("0" + val);
            else if (val.Length < 3)
                return ("00" + val);
            else
                return val;
        }
    }
}
