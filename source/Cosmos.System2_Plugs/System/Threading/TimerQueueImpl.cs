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
