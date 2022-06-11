using System;
using System.Threading.Tasks;
using IL2CPU.API.Attribs;

namespace Cosmos.Core_Plugs.System.Threading.Tasks;

[Plug(typeof(TaskScheduler))]
internal class TaskSchedulerImpl
{
    public static void cctor() => throw new NotImplementedException();
}
