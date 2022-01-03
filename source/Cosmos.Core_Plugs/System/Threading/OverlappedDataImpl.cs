using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using IL2CPU.API.Attribs;

namespace Cosmos.Core_Plugs.System.Threading
{
    [Plug(TargetName = "System.Threading.OverlappedData, System.Private.CoreLib")]
    class OverlappedDataImpl
    {
        [PlugMethod(Signature = "System_Threading_OverlappedData__System_Threading_OverlappedData_GetOverlappedFromNative_System_Threading_NativeOverlapped__")]
        public unsafe static object GetOverlappedFromNative(NativeOverlapped* a)
        {
            throw new NotImplementedException();
        }

        [PlugMethod(Signature = "System_Void__System_Threading_OverlappedData_FreeNativeOverlapped_System_Threading_NativeOverlapped__")]
        public unsafe static void FreeNativeOverlaapped(NativeOverlapped* a)
        {
            throw new NotImplementedException();
        }
    }
}
