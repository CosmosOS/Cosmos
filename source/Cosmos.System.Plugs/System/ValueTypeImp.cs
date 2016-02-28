using System;
using Cosmos.Common;
using Cosmos.IL2CPU.Plugs;

namespace Cosmos.System.Plugs.System
{
    [Plug(Target = typeof(ValueType))]
    public static class ValueTypeImp
    {
        public static int GetHashCode(ValueType aThis)
        {
            if (aThis is byte)
                return (int)aThis;

            return -1; 
        }

        public static int GetHashCodeOfPtr(IntPtr ptr)
        {
            throw new NotImplementedException("ValueType.GetHashCodeOfPtr()");
        }

    }
}
