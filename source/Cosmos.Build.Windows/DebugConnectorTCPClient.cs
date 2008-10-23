using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Text;
using System.Windows.Threading;

namespace Cosmos.Compiler.Builder {
    public class DebugConnectorTCPClient : DebugConnectorStream {

        public DebugConnectorTCPClient() {
            var xTCPClient = new TcpClient("localhost", 4444);
            Start(xTCPClient.GetStream());
        }

        
    }
}
