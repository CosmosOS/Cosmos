using IL2CPU.API.Attribs;

namespace Cosmos.System_Plugs.System.Threading
{
    [Plug(typeof(CancellationTokenSource))]
    public static class CancellationTokenSourceImpl
    {
        public static void NotifyCancellation(CancellationTokenSource aThis, bool aBool)
        {
            throw new NotImplementedException();
        }
        public static void Dispose(CancellationTokenSource aThis)
        {
            throw new NotImplementedException();
        }
    }
}