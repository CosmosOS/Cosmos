using IL2CPU.API.Attribs;

namespace Cosmos.Core_Plugs.System.Threading.Tasks;

[Plug("System.Threading.Tasks.ThreadPoolTaskScheduler, System.Private.CoreLib")]
internal class ThreadPoolTaskSchedulerImpl
{
    public static void cctor()
    {
    }
}
