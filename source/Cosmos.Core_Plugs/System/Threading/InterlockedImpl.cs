using System.Threading;
using IL2CPU.API.Attribs;

namespace Cosmos.Core_Plugs.System.Threading
{
    [Plug(Target = typeof(Interlocked))]
    public static class InterlockedImpl
    {
        public static int Decrement(ref int aData)
        {
            return aData -= 1;
        }
    }
}
