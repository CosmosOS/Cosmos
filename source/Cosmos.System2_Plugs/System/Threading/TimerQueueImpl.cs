using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IL2CPU.API.Attribs;

namespace Cosmos.System_Plugs.System.Threading
{
    class TimerQueueImpl
    {
    }
    [Plug(TargetName= "System.Threading.TimerQueue+AppDomainTimerSafeHandle, System.Private.CoreLib")]
    public class AppDomainTimerSafeHandleImpl
    {
        public static bool ReleaseHandle(object aThis)
        {
            throw new NotImplementedException();
        }
    }
}
