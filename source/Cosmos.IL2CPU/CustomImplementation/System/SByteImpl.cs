using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cosmos.IL2CPU.Plugs;

namespace Cosmos.IL2CPU.CustomImplementation.System
{
    [Plug(Target=typeof(sbyte))]
    public static class SByteImpl
    {
        public static string ToString(ref sbyte aThis)
        {
            return unchecked((Int32)(aThis)).ToString();
        }
    }
}
