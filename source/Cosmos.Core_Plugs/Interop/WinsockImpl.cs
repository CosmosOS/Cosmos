using IL2CPU.API.Attribs;
using System;
using System.Net.Sockets;

namespace Cosmos.Core_Plugs.Interop
{
    [Plug("Interop+Winsock, System.Net.Sockets", IsOptional = true)]
    public static unsafe class WinsockImpl
    {
        public static SocketError shutdown(SafeSocketHandle socketHandle, int how)
        {
            throw new NotImplementedException();
        }

        public static int recv(SafeSocketHandle socketHandle, byte* pinnedBuffer, int len, SocketFlags socketFlags)
        {
            throw new NotImplementedException();
        }

        public static int send(SafeSocketHandle socketHandle, byte* pinnedBuffer, int len, SocketFlags socketFlags)
        {
            throw new NotImplementedException();
        }

        public static int bind(SafeSocketHandle socketHandle, byte[] socketAddress, int socketAddressSize)
        {
            throw new NotImplementedException();
        }
    }
}
