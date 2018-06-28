using System;
using Cosmos.Core;
using IL2CPU.API;
using IL2CPU.API.Attribs;

namespace Cosmos.Core_Plugs.System
{
    [Plug(Target = typeof(DateTime))]
    public class DateTimeImpl
    {

        public static DateTime get_Now()
        {
            int[] raw = GetRawDate();

            return new DateTime(
                100 * BCDtoBIN(raw[10]) + BCDtoBIN(raw[9]), //YEAR
                BCDtoBIN(raw[8]), //MONTH
                BCDtoBIN(raw[7]), //DAY
                BCDtoBIN(raw[4]), //HOUR
                BCDtoBIN(raw[2]), //MINUTE
                BCDtoBIN(raw[0]) //SECOND
            );
        }

        // TODO: get timezone
        public static DateTime get_UtcNow() => get_Now();

        private static int[] GetRawDate()
        {
            IOPort p70 = new IOPort(0x70);
            IOPort p71 = new IOPort(0x71);
            int[] raw = new int[0x0b];

            p70.Byte = 0x0b;
            p71.Byte = 0x02; //24h format + BCD encoding

            do
                p70.Byte = 0x0a;
            while ((p71.Byte & 0x80) == 0);

            for (byte i = 0; i < 0x0a; i++)
            {
                p70.Byte = i;

                raw[i] = p71.Word;
            }

            p70.Byte = 0x32;

            raw[0x0a] = p71.Word;

            return raw;
        }

        public static int ToString(ref decimal aThis)
        {
            throw new NotImplementedException("DateTime.ToString()");
        }

        public static long GetSystemTimeAsFileTime() => get_Now().Ticks;

        /// <summary>
        /// Converts BCD-encoded numbers to `normal` (binary-encoded) numbers
        /// </summary>
        private static int BCDtoBIN(int bcd) => ((bcd & 0xf0) >> 1) + ((bcd & 0xf0) >> 3) + (bcd & 0xf);
    }
}
