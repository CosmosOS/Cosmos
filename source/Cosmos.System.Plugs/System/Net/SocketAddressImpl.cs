using System.Net;

using Cosmos.IL2CPU.Plugs;

namespace Cosmos.System.Plugs.System.Net
{
    [Plug(Target = typeof(SocketAddress))]
    public static class SocketAddressImpl
    {
        public static string ToString(SocketAddress aThis)
        {
            return "<SocketAddress.ToString() not yet plugged correctly!>";
        }
    }
}