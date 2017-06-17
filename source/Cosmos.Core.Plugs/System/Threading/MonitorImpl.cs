using System.Threading;
using Cosmos.IL2CPU.Plugs;

namespace Cosmos.Core.Plugs.System.Threading
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
