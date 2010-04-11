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
        }

        public override void WaitConnect() {
            NamedPipeClientStream xPipe = new NamedPipeClientStream("CosmosDebug");
            // No need to loop, this waits infinitely and can be called before server side is ready
            xPipe.Connect();
            Start(xPipe);
        }
    }
}
