using Cosmos.Common;
using Cosmos.IL2CPU.Plugs;

namespace Cosmos.IL2CPU.IL.CustomImplementations.System
{
    [Plug(Target = typeof(ushort))]
    public static class UInt16Impl
    {
        public static string ToString(ref ushort aThis)
        {
            return StringHelper.GetNumberString(aThis, false);
        }
    }
}