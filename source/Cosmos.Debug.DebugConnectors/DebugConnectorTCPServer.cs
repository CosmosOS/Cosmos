using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Cosmos.Debug.DebugConnectors
{
    /// Used for GDB.
    public class DebugConnectorTCPServer : DebugConnectorStreamWithTimeouts
    {
        private TcpClient mClient;

        public DebugConnectorTCPServer()
        {
            var xTCPListener = new TcpListener(IPAddress.Loopback, 4444);

            xTCPListener.Start();

            var xTask = xTCPListener.AcceptTcpClientAsync();

            xTask.ContinueWith(DoAcceptTcpClientCallback);
        }

        protected override void InitializeBackground()
        {
            throw new NotImplementedException();
        }

        public void DoAcceptTcpClientCallback(Task<TcpClient> aTask)
        {
            var xTCPListener = (TcpListener)aTask.AsyncState;
            mClient = aTask.Result;
            Start(mClient.GetStream());
        }

        protected override bool GetIsConnectedToDebugStub()
        {
            return mClient != null
                   && mClient.Connected;
        }
    }
}
