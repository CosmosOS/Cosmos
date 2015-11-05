using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cosmos.IL2CPU.Plugs;

namespace Cosmos.Kernel.Plugs
{
    [Plug(Target = typeof(System.Threading.Interlocked))]
    public static class Interlocked
    {
        public static object CompareExchange(ref object location1, object value, object comparand)
        {
            object xResult = null;
            CPU.DisableInterrupts();
            try
            {
                xResult = location1;
                if (Object.ReferenceEquals(location1, comparand))
                {
                    location1 = value;
                }
            }
            finally
            {
                CPU.EnableInterrupts();
            }
            return xResult;
        }
    }
}