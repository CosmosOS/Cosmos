using System;

using Cosmos.Common;

using IL2CPU.API.Attribs;

namespace Cosmos.System_Plugs.System
{
    [Plug(Target = typeof(ulong))]
    public class UInt64Impl
    {
        public static string ToString(ref ulong aThis) => StringHelper.GetNumberString(aThis);

        public static string ToString(ref ulong aThis, string format, IFormatProvider provider) => aThis.ToString();
    }
}
