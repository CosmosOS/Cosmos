using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using IL2CPU.API.Attribs;

namespace Cosmos.Core_Plugs.System.Threading
{
    class OverlappedDataImpl
    {
        [PlugMethod(Signature = "System_Threading_OverlappedData__System_Threading_OverlappedData_GetOverlappedFromNative_System_Threading_NativeOverlapped__")]
        public unsafe static void GetOverlappedFromNative(NativeOverlapped* a)
        {
            throw new NotImplementedException();
        }
    }
}
