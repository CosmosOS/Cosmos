using Cosmos.Common;
using Cosmos.IL2CPU.API;
using Cosmos.IL2CPU.API.Attribs;

namespace Cosmos.System_Plugs.System
{
    [Plug(Target = typeof(sbyte))]
    public static class SByteImpl
    {
        public static string ToString(ref sbyte aThis)
        {
            return StringHelper.GetNumberString(aThis);
        }
    }
}
