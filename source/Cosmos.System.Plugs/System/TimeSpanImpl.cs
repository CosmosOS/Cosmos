using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cosmos.IL2CPU.Plugs;

namespace Cosmos.System.Plugs
{
    [Plug(Target = typeof(global::System.TimeSpan))]
    public class TimeSpanImpl
    {
        public static string ToString(ref TimeSpan aThis)
        {
            return "ts";
        }
    }
}
