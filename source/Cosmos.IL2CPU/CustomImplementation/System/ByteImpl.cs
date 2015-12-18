using Cosmos.Common;
using Cosmos.IL2CPU.Plugs;

namespace Cosmos.IL2CPU.IL.CustomImplementations.System
{
    [Plug(Target = typeof(byte))]
    public static class ByteImpl
    {
        public static string ToString(ref byte aThis)
        {
            return StringHelper.GetNumberString(aThis, false);
        }
    }
}