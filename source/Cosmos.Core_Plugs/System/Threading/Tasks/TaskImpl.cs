using System.Threading;
using System.Threading.Tasks;

using IL2CPU.API.Attribs;

namespace Cosmos.Core_Plugs.System.Threading.Tasks
{
    [Plug(Target = typeof(Task))]
    public static class TaskImpl
    {
        public static void Cctor()
        {
        }

        public static void Ctor()
        {
        }

        public static void Dispose(Task aThis, bool disposing)
        {
        }
    }
}
