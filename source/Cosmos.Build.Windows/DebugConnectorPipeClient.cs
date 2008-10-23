using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Windows.Threading;

namespace Cosmos.Compiler.Builder {
    public class DebugConnectorPipeClient : DebugConnectorStream {
    
        public DebugConnectorPipeClient() {
            NamedPipeClientStream xPipe = new NamedPipeClientStream("CosmosDebug");
            xPipe.Connect();
            Start(xPipe);
        }

    }
}
