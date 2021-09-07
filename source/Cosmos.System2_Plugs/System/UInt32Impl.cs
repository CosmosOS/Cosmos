using System;
using Cosmos.Common;
using IL2CPU.API;
using IL2CPU.API.Attribs;

namespace Cosmos.System_Plugs.System
{
    [Plug(Target = typeof(UInt32))]
    public static class UInt32Impl
    {
        public static string ToString(ref uint aThis)
        {
            return StringHelper.GetNumberString(aThis);
        }
    }
}
