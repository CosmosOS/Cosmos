using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using IL2CPU.API.Attribs;

namespace Cosmos.Core_Plugs.System.Threading
{
    [Plug(typeof(global::System.Threading.EventWaitHandle))]
    class EventWaitHandleImpl
    {
        public static bool Set(EventWaitHandle aThis)
        {
            throw new NotImplementedException();
        }
    }
}
