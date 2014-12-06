using Cosmos.IL2CPU.Plugs;
using System.Net;

namespace Cosmos.IL2CPU.IL.CustomImplementations.System.Net
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