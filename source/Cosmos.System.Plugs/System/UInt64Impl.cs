using System;
using Cosmos.Common;
using Cosmos.IL2CPU.Plugs;

namespace Cosmos.System.Plugs.System
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
