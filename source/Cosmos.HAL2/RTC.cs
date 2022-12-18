using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cosmos.Core;

namespace Cosmos.HAL
{
    /// <summary>
    /// This class represents the Real-Time Clock.
    /// </summary>
    public static class RTC
    {
        /// <summary>
        /// Address IOPort.
        /// </summary>
        public const int Address = 0x70;
        /// <summary>
        /// Data IOPort.
        /// </summary>
        public const int Data = 0x71;

        /// <summary>
        /// The static constructor for RTC,
        /// initializes all of the options
        /// that are possible in the RTC.
        /// </summary>
        static RTC()
        {
            WaitForReady();
            IOPort.Write8(Address, 0x0B);
            StatusByteB = IOPort.Read8(Data);

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
                IOPort.Write8(Address, 0);
                var data = IOPort.Read8(Data);

                if (isBCDMode)
                {
                    return FromBCD(data);
                }
                else
                {
                    return data;
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
                IOPort.Write8(Address, 2);
                var data = IOPort.Read8(Data);

                if (isBCDMode)
                {
                    return FromBCD(data);
                }
                else
                {
                    return data;
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
                IOPort.Write8(Address, 4);
                var data = IOPort.Read8(Data);

                if (isBCDMode)
                {
                    if (is24HourMode)
                    {
                        return FromBCD(data);
                    }
                    else
                    {
                        if ((data & 0x80) == 0x80)
                        {
                            // It's PM.
                            return (byte)(FromBCD(data) + 12);
                        }
                        else
                        {
                            if (FromBCD(data) == 12)
                            {
                                // It's midnight, so it should actually be 0.
                                return 0;
                            }
                            else
                            {
                                return FromBCD(data);
                            }
                        }
                    }
                }
                else
                {
                    if (is24HourMode)
                    {
                        return data;
                    }
                    else
                    {
                        if ((data & 0x80) == 0x80)
                        {
                            // It's PM.
                            return (byte)(data + 12);
                        }
                        else
                        {
                            if (data == 12)
                            {
                                // It's midnight, so it should actually be 0.
                                return 0;
                            }
                            else
                            {
                                return data;
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
                IOPort.Write8(Address, 6);
                var data = IOPort.Read8(Data);

                if (isBCDMode)
                {
                    return FromBCD(data);
                }
                else
                {
                    return data;
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
                IOPort.Write8(Address, 7);
                var data = IOPort.Read8(Data);

                if (isBCDMode)
                {
                    return FromBCD(data);
                }
                else
                {
                    return data;
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
                IOPort.Write8(Address, 8);
                var data = IOPort.Read8(Data);

                if (isBCDMode)
                {
                    return FromBCD(data);
                }
                else
                {
                    return data;
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
                IOPort.Write8(Address, 9);
                var data = IOPort.Read8(Data);

                if (isBCDMode)
                {
                    return FromBCD(data);
                }
                else
                {
                    return data;
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
                IOPort.Write8(Address, 0x32);
                var data = IOPort.Read8(Data);

                if (isBCDMode)
                {
                    return FromBCD(data);
                }
                else
                {
                    return data;
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
            return (byte)((value / 16 * 10) + (value & 0xF));
        }

        /// <summary>
        /// Waits until the status register says it can accept a value.
        /// </summary>
        private static void WaitForReady()
        {
            do
            {
                IOPort.Write8(Address, 10);
            }
            while ((IOPort.Read8(Data) & 0x80) != 0);
        }
    }
}
