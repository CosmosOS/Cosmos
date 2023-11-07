using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using IL2CPU.API.Attribs;

namespace Cosmos.Core_Plugs.System.Threading
{
    [Plug(TargetName = "System.Threading.Overlapped, System.Private.CoreLib")]
    class OverlappedImpl
    {
        [PlugMethod(Signature = "System_Void__System_Threading_Overlapped_Free_System_Threading_NativeOverlapped__")]
        public unsafe static void Free(NativeOverlapped* a)
        {
            throw new NotImplementedException();
        }
    }
}
