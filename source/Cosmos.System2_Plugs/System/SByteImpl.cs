using Cosmos.Common;
using IL2CPU.API.Attribs;

namespace Cosmos.System_Plugs.System
{
    [Plug(Target = typeof(sbyte))]
    public static class SByteImpl
    {
        public static string ToString(ref sbyte aThis) => StringHelper.GetNumberString(aThis);

        public static string ToString(ref sbyte aThis, string format, IFormatProvider provider) => aThis.ToString();
    }
}