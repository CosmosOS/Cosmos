using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using IL2CPU.API.Attribs;

namespace Cosmos.Core_Plugs.System.Threading
{
    [Plug(TargetName = "System.Threading.ThreadPoolBoundHandleOverlapped, System.Private.CoreLib")]
    class ThreadPoolBoundHandleOverlapped
    {
        public static void Cctor()
        {

        }
    }
}
