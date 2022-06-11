using System;
using Cosmos.Common;
using IL2CPU.API.Attribs;

namespace Cosmos.System_Plugs.System;

[Plug(Target = typeof(uint))]
public static class UInt32Impl
{
    public static string ToString(ref uint aThis) => StringHelper.GetNumberString(aThis);

    public static string ToString(ref uint aThis, string format, IFormatProvider provider) => aThis.ToString();
}
