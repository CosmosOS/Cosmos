using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using IL2CPU.API.Attribs;

namespace Cosmos.System_Plugs.System.Net.Sockets
{
    [Plug(Target = typeof(Socket))]
    public static class SocketImpl
    {
        public static void Dispose(Socket aThis)
        {
            throw new NotImplementedException();
        }

        public static void Dispose(Socket aThis, bool disposing)
        {
            throw new NotImplementedException();
        }

        #region Plugs

        public static bool AcceptAsync(Socket aThis, object e)
        {
            throw new NotImplementedException();
        }

        public static bool AcceptAsync(Socket aThis, object e, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public static bool ConnectAsync(Socket aThis, object e)
        {
            throw new NotImplementedException();
        }

        public static bool ConnectAsync(Socket aThis, object e, bool userSocket, bool saeaCancelable)
        {
            throw new NotImplementedException();
        }

        public static bool ConnectAsync(SocketType socketType, ProtocolType protocolType, object e)
        {
            throw new NotImplementedException();
        }

        public static void CancelConnectAsync(object e)
        {
            throw new NotImplementedException();
        }

        public static bool DisconnectAsync(Socket aThis, object e)
        {
            throw new NotImplementedException();
        }

        public static bool DisconnectAsync(Socket aThis, object e, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public static bool ReceiveAsync(Socket aThis, object e)
        {
            throw new NotImplementedException();
        }

        public static bool ReceiveAsync(Socket aThis, object e, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public static bool ReceiveFromAsync(Socket aThis, object e)
        {
            throw new NotImplementedException();
        }

        public static bool ReceiveFromAsync(Socket aThis, object e, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public static bool ReceiveMessageFromAsync(Socket aThis, object e)
        {
            throw new NotImplementedException();
        }

        public static bool ReceiveMessageFromAsync(Socket aThis, object e, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public static bool SendAsync(Socket aThis, object e)
        {
            throw new NotImplementedException();
        }

        public static bool SendAsync(Socket aThis, object e, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public static bool SendPacketsAsync(Socket aThis, object e)
        {
            throw new NotImplementedException();
        }

        public static bool SendPacketsAsync(Socket aThis, object e, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public static bool SendToAsync(Socket aThis, object e)
        {
            throw new NotImplementedException();
        }

        public static bool SendToAsync(Socket aThis, object e, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
