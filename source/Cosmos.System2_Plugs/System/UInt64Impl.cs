using System;
using Cosmos.Common;
using Cosmos.Common.Extensions;
using IL2CPU.API.Attribs;

namespace Cosmos.System_Plugs.System;

[Plug(Target = typeof(ulong))]
public class UInt64Impl
{
    public static string ToString(ref ulong aThis) => StringHelper.GetNumberString(aThis);

    public static string ToString(ref ulong aThis, string formating)
    {
        if (formating == "X")
        {
            return aThis.ToHex(false);
        }

        if (formating == "G")
        {
            return ToString(ref aThis);
        }

        throw new NotImplementedException();
    }
}
