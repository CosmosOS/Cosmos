using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Text;

namespace Cosmos.Debug.Common.CDebugger
{
    public class DebugConnectorPipeClient : DebugConnectorStream {
    
        public DebugConnectorPipeClient() {
            NamedPipeClientStream xPipe = new NamedPipeClientStream("CosmosDebug");
            xPipe.Connect();
            Start(xPipe);
        }

    }
}
