using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Text;

namespace Cosmos.Debug.Common.CDebugger
{
    public class DebugConnectorPipeServer : DebugConnectorStream {
    
        public DebugConnectorPipeServer() {
            NamedPipeServerStream xPipe = new NamedPipeServerStream("CosmosDebug", PipeDirection.InOut, 1
             , PipeTransmissionMode.Byte, PipeOptions.Asynchronous);
            xPipe.BeginWaitForConnection(new AsyncCallback(DoWaitForConnection), xPipe);
        }

        public void DoWaitForConnection(IAsyncResult aResult) {
            var xPipe = (NamedPipeServerStream)aResult.AsyncState;
            xPipe.EndWaitForConnection(aResult);
            Start(xPipe);
        }

        public override void WaitConnect() {
            throw new NotImplementedException();
        }
    }
}
