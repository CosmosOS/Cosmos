using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IL2CPU.API.Attribs;

namespace Cosmos.Core_Plugs.Runtime.Intrinsics.X86
{
    [Plug("System.Runtime.Intrinsics.X86.Lzcnt, System.Private.CoreLib")]
    class LzcntImpl
    {
        public static bool get_IsSupported()
        {
            return false;
        }
    }

    [Plug("System.Runtime.Intrinsics.X86.Lzcnt+X64, System.Private.CoreLib")]
    class X64LzcntImpl
    {
        public static bool get_IsSupported()
        {
            return false;
        }
    }
}
