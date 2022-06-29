using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IL2CPU.API.Attribs;

namespace Cosmos.Core_Plugs.System.Threading.Tasks
{
    [Plug(typeof(global::System.Threading.Tasks.TaskScheduler))]
    class TaskSchedulerImpl
    {
        public static void cctor()
        {
            throw new NotImplementedException();
        }
    }
}
