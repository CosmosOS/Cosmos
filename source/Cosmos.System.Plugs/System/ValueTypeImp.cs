using System;
using Cosmos.Common;
using Cosmos.IL2CPU.Plugs;

namespace Cosmos.System.Plugs.System
{
    [Plug(Target = typeof(ValueType))]
    class ValueTypeImp
    {
#if false
        public static int GetHashCode(ValueType aThis)
        {
            if (aThis is byte)
                return (int)aThis;

            return -1;
        }
#endif
    }
}
