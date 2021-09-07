using System.Threading;
using IL2CPU.API.Attribs;
using System.Threading.Tasks;

namespace Cosmos.Core_Plugs.System.Threading
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
