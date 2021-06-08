using System;

using Cosmos.Common;

using IL2CPU.API.Attribs;

namespace Cosmos.System_Plugs.System
{
    [Plug(Target = typeof(ushort))]
    public static class UInt16Impl
    {
        public static string ToString(ref ushort aThis) => StringHelper.GetNumberString(aThis);

        public static string ToString(ref ushort aThis, string format, IFormatProvider provider) => aThis.ToString();
    }
}
