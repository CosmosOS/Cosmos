using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IL2CPU.API.Attribs;

namespace Cosmos.Core_Plugs.System.Threading.Tasks
{
    [Plug("System.Threading.Tasks.ThreadPoolTaskScheduler, System.Private.CoreLib")]
    class ThreadPoolTaskSchedulerImpl
    {
        public static void cctor()
        {

        }
    }
}
