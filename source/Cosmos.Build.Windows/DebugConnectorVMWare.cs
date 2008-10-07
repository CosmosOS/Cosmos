using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Windows.Threading;

namespace Cosmos.Compiler.Builder {
    public class DebugConnectorVMWare : DebugConnectorStream {
    
        public DebugConnectorVMWare() {
            NamedPipeServerStream xPipe = new NamedPipeServerStream("CosmosDebug", PipeDirection.InOut, 1
             , PipeTransmissionMode.Byte, PipeOptions.Asynchronous);
            xPipe.BeginWaitForConnection(new AsyncCallback(DoWaitForConnection), xPipe);
        }

        public void DoWaitForConnection(IAsyncResult aResult) {
            var xPipe = (NamedPipeServerStream)aResult.AsyncState;
            xPipe.EndWaitForConnection(aResult);
            Start(xPipe);
        }

    }
}
