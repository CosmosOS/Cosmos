using System;
using System.Net;
using System.Net.Sockets;

namespace Cosmos.Debug.Common
{
    /// Used for GDB.
    public class DebugConnectorTCPServer : DebugConnectorStream
    {
        public DebugConnectorTCPServer()
        {
            var xTCPListener = new TcpListener(IPAddress.Loopback, 4444);
            xTCPListener.Start();
            xTCPListener.BeginAcceptTcpClient(new AsyncCallback(DoAcceptTcpClientCallback), xTCPListener);
        }

        public void DoAcceptTcpClientCallback(IAsyncResult aResult)
        {
            var xListener = (TcpListener)aResult.AsyncState;
            var xClient = xListener.EndAcceptTcpClient(aResult);
            Start(xClient.GetStream());
        }
    }
}