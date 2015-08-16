using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Text;
using System.Threading;

namespace Cosmos.Debug.Common {
  /// Used for GDB.
  public class DebugConnectorTCPServer : DebugConnectorStreamWithTimeouts {
      private TcpClient mClient;

      public DebugConnectorTCPServer() {
      var xTCPListener = new TcpListener(IPAddress.Loopback, 4444);
      xTCPListener.Start();
      xTCPListener.BeginAcceptTcpClient(new AsyncCallback(DoAcceptTcpClientCallback), xTCPListener);
    }

      protected override void InitializeBackground()
      {
          throw new NotImplementedException();
      }

      public void DoAcceptTcpClientCallback(IAsyncResult aResult) {
      var xListener = (TcpListener)aResult.AsyncState;
      mClient = xListener.EndAcceptTcpClient(aResult);
      Start(mClient.GetStream());
    }

      protected override bool GetIsConnectedToDebugStub()
      {
          return mClient != null
                 && mClient.Connected;
      }
  }
}
