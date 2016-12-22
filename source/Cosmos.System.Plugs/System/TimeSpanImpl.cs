using System;

using Cosmos.IL2CPU.Plugs;

namespace Cosmos.System.Plugs.System
{
    [Plug(Target = typeof(TimeSpan))]
    public static class TimeSpanImpl
    {
        public static string ToString(ref TimeSpan aThis)
        {
            string xTime = "";

            if (aThis.Ticks < 0)
            {
                xTime += '-';
            }

            if (aThis.Days != 0)
            {
                xTime += aThis.Days.ToString() + ".";
            }

            xTime += aThis.Hours.ToString().PadLeft(2, '0') + ":" + aThis.Minutes.ToString().PadLeft(2, '0') + ":" + aThis.Seconds.ToString().PadLeft(2, '0');

            if ((aThis.Ticks - (long)aThis.TotalSeconds * 1000 * 10000) != 0)
            {
                xTime += "." + (aThis.Ticks - (long)aThis.TotalSeconds * 1000 * 10000).ToString().PadLeft(7, '0');
            }

            return xTime;
        }
    }
}
