using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IL2CPU.API.Attribs;

namespace Cosmos.Core_Plugs.Microsoft
{
    [Plug("Microsoft.Win32.SafeHandles.SafeFileHandle+OverlappedValueTaskSource, System.Private.CoreLib", IsOptional = true)]
    public static class OverlappedValueTaskSourceImpl
    {
        [PlugMethod(Signature = "System_Void__Microsoft_Win32_SafeHandles_SafeFileHandle_OverlappedValueTaskSource__cctor__")]
        public static void Cctor()
        {
            throw new NotImplementedException();
        }
    }
}
