using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Text;

namespace Cosmos.Debug.Common.CDebugger
{
    public class DebugConnectorTCPClient : DebugConnectorStream {

        public DebugConnectorTCPClient() {
            var xTCPClient = new TcpClient("localhost", 4444);
            Start(xTCPClient.GetStream());
        }

        public override void WaitConnect() {
            throw new NotImplementedException();
        }
    }
}