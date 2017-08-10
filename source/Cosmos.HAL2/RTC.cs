using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.HAL
{
    /// <summary>
    /// This class represents the Real-Time Clock.
    /// </summary>
    public static class RTC
    {
        private static Core.IOGroup.RTC rtc = new Core.IOGroup.RTC();

        /// <summary>
        /// The static constructor for RTC,
        /// initializes all of the options
        /// that are possible in the RTC.
        /// </summary>
        static RTC()
        {
            WaitForReady();
            rtc.Address.Byte = 0x0B;
            StatusByteB = rtc.Data.Byte;

            #region Check 24-Hour Mode
            if ((StatusByteB & 0x02) == 0x02)
            {
                is24HourMode = true;
            }
            else
            {
                is24HourMode = false;
            }
            #endregion

            #region Check Binary Mode
            if ((StatusByteB & 0x04) == 0x04)
            {
                // Binary mode.
                isBCDMode = false;
            }
            else
            {
                isBCDMode = true;
            }
            #endregion


        }

        /// <summary>
        /// True if the RTC is using BCD mode.
        /// </summary>
        private static bool isBCDMode;
        /// <summary>
        /// True if the RTC is using the 24-hour format.
        /// </summary>
        private static bool is24HourMode;
        /// <summary>
        /// The value of Status Register B that was read on startup.
        /// </summary>
        private static byte StatusByteB;

        /// <summary>
        /// The current second.
        /// </summary>
        public static byte Second
        {
            get
            {
                WaitForReady();
                rtc.Address.Byte = 0;
                if (isBCDMode)
                {
                    return FromBCD(rtc.Data.Byte);
                }
                else
                {
                    return rtc.Data.Byte;
                }
            }
        }

        /// <summary>
        /// The current minute.
        /// </summary>
        public static byte Minute
        {
            get
            {
                WaitForReady();
                rtc.Address.Byte = 2;
                if (isBCDMode)
                {
                    return FromBCD(rtc.Data.Byte);
                }
                else
                {
                    return rtc.Data.Byte;
                }
            }
        }

        /// <summary>
        /// The current hour. Please note, this is 
        /// always in 24-hour format.
        /// </summary>
        public static byte Hour
        {
            get
            {
                WaitForReady();
                rtc.Address.Byte = 4;
                if (isBCDMode)
                {
                    if (is24HourMode)
                    {
                        return FromBCD(rtc.Data.Byte);
                    }
                    else
                    {
                        byte b = rtc.Data.Byte;
                        if ((b & 0x80) == 0x80)
                        {
                            // It's PM.
                            return (byte)(FromBCD(b) + 12);
                        }
                        else
                        {
                            if (FromBCD(b) == 12)
                            {
                                // It's midnight, so it should actually be 0.
                                return 0;
                            }
                            else
                            {
                                return FromBCD(b);
                            }
                        }
                    }
                }
                else
                {
                    if (is24HourMode)
                    {
                        return rtc.Data.Byte;
                    }
                    else
                    {
                        byte b = rtc.Data.Byte;
                        if ((b & 0x80) == 0x80)
                        {
                            // It's PM.
                            return (byte)(b + 12);
                        }
                        else
                        {
                            if (b == 12)
                            {
                                // It's midnight, so it should actually be 0.
                                return 0;
                            }
                            else
                            {
                                return b;
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// The current day of the week.
        /// </summary>
        public static byte DayOfTheWeek
        {
            get
            {
                WaitForReady();
                rtc.Address.Byte = 6;
                if (isBCDMode)
                {
                    return FromBCD(rtc.Data.Byte);
                }
                else
                {
                    return rtc.Data.Byte;
                }
            }
        }

        /// <summary>
        /// The current day of the month.
        /// </summary>
        public static byte DayOfTheMonth
        {
            get
            {
                WaitForReady();
                rtc.Address.Byte = 7;
                if (isBCDMode)
                {
                    return FromBCD(rtc.Data.Byte);
                }
                else
                {
                    return rtc.Data.Byte;
                }
            }
        }

        /// <summary>
        /// The current month.
        /// </summary>
        public static byte Month
        {
            get
            {
                WaitForReady();
                rtc.Address.Byte = 8;
                if (isBCDMode)
                {
                    return FromBCD(rtc.Data.Byte);
                }
                else
                {
                    return rtc.Data.Byte;
                }
            }
        }

        /// <summary>
        /// The current year in the century.
        /// </summary>
        public static byte Year
        {
            get
            {
                WaitForReady();
                rtc.Address.Byte = 9;
                if (isBCDMode)
                {
                    return FromBCD(rtc.Data.Byte);
                }
                else
                {
                    return rtc.Data.Byte;
                }
            }
        }

        /// <summary>
        /// The current century. Beware, this may cause issues 
        /// on computers from before 1995.
        /// </summary>
        public static byte Century
        {
            get
            {
                WaitForReady();
                rtc.Address.Byte = 0x32;
                if (isBCDMode)
                {
                    return FromBCD(rtc.Data.Byte);
                }
                else
                {
                    return rtc.Data.Byte;
                }
            }
        }

        /// <summary>
        /// Converts a BCD coded value to hex coded 
        /// </summary>
        /// <param name="value">BCD coded</param>
        /// <returns>Hex coded</returns>
        private static byte FromBCD(byte value)
        {
            return (byte)(((value >> 4) & 0x0F) * 10 + (value & 0x0F));
        }

        /// <summary>
        /// Waits until the status register says it can accept a value.
        /// </summary>
        private static void WaitForReady()
        {
            do
            {
                rtc.Address.Byte = 10;
            }
            while ((rtc.Data.Byte & 0x80) != 0);
        }
    }
}
