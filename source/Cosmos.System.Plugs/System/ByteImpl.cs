using System;
using Cosmos.Common;
using Cosmos.IL2CPU.Plugs;

namespace Cosmos.System.Plugs.System
{
    [Plug(Target = typeof(byte))]
    public static class ByteImpl
    {
        public static string ToString(ref byte aThis)
        {
            return StringHelper.GetNumberString(aThis);
        }

        public static Int32 GetHashCode(ref byte aThis)
        {
            return aThis;
        }
    }
}
