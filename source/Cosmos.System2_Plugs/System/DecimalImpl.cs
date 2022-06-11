using System;
using IL2CPU.API.Attribs;

namespace Cosmos.System_Plugs.System;

[Plug(Target = typeof(decimal))]
public static class DecimalImpl
{
    public static bool Equals(ref decimal aThis, object value) => throw new NotImplementedException("Decimal.Equals()");

    public static int GetHashCode(ref decimal aThis) => throw new NotImplementedException("Decimal.GetHashCode()");

    public static string ToString(ref decimal aThis) => throw new NotImplementedException("Decimal.ToString()");

    public static string ToString(ref decimal aThis, string format, IFormatProvider provider) => aThis.ToString();
}
