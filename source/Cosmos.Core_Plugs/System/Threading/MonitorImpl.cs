using System.Threading;
using IL2CPU.API.Attribs;

namespace Cosmos.Core_Plugs.System.Threading
{
    [Plug(Target = typeof(Monitor))]
    public class MonitorImpl
    {
        public static void Enter(object aObj)
        {
        }

        public static void Exit(object aObj)
        {
        }

        public static void ReliableEnter(object aObj, ref bool aTookLock)
        {
            aTookLock = true;
        }
    }
}
