using Cosmos.Common;
using Cosmos.IL2CPU.API;
using Cosmos.IL2CPU.API.Attribs;

namespace Cosmos.System_Plugs.System
{
    [Plug(Target = typeof(ushort))]
    public static class UInt16Impl
    {
        public static string ToString(ref ushort aThis)
        {
            return StringHelper.GetNumberString(aThis);
        }
    }
}
