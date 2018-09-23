using System;

using IL2CPU.API.Attribs;

namespace Cosmos.System_Plugs.System
{
    [Plug(typeof(TimeSpan))]
    public static class TimeSpanImpl
    {
        public static string ToString(ref TimeSpan aThis)
        {
            string time = null;

            if (aThis.Ticks < 0)
            {
                time += '-';
            }

            if (aThis.Days != 0)
            {
                time += aThis.Days.ToString() + ".";
            }

            time += aThis.Hours.ToString().PadLeft(2, '0') + ":" +
                    aThis.Minutes.ToString().PadLeft(2, '0') + ":" +
                    aThis.Seconds.ToString().PadLeft(2, '0');

            if ((aThis.Ticks - (long)aThis.TotalSeconds * 1000 * 10000) != 0)
            {
                time += "." + (aThis.Ticks - (long)aThis.TotalSeconds * 1000 * 10000).ToString().PadLeft(7, '0');
            }

            return time;
        }
    }
}
