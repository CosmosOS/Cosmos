using System;
using Cosmos.Common;
using IL2CPU.API.Attribs;

namespace Cosmos.System_Plugs.System;

[Plug(Target = typeof(byte))]
public static class ByteImpl
{
    public static string ToString(ref byte aThis) => StringHelper.GetNumberString(aThis);

    public static string ToString(ref byte aThis, string format, IFormatProvider provider) => aThis.ToString();
}
