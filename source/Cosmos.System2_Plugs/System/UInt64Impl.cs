using System;
using Cosmos.Common;
using IL2CPU.API;
using IL2CPU.API.Attribs;

namespace Cosmos.System_Plugs.System
{
    [Plug(Target = typeof(UInt64))]
    public class UInt64Impl
    {
        public static string ToString(ref ulong aThis)
        {
            return StringHelper.GetNumberString(aThis);
        }
    }
}
