using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading;

namespace Cosmos.Debug.Common
{
	/// <summary>
	/// Used for Vmware.
	/// </summary>
    public class DebugConnectorPipeServer : DebugConnectorStream {
    
        public DebugConnectorPipeServer() {
            mPipe = new NamedPipeServerStream("CosmosDebug", PipeDirection.InOut, 1
             , PipeTransmissionMode.Byte, PipeOptions.Asynchronous);
            mPipe.BeginWaitForConnection(new AsyncCallback(DoWaitForConnection), mPipe);
        }

        private AutoResetEvent mWaitConnectEvent = new AutoResetEvent(false);
        private NamedPipeServerStream mPipe;

        public void DoWaitForConnection(IAsyncResult aResult) {
            var xPipe = (NamedPipeServerStream)aResult.AsyncState;
            xPipe.EndWaitForConnection(aResult);
            mWaitConnectEvent.Set();
            Start(xPipe);
        }

        public override void WaitConnect() {
            mWaitConnectEvent.WaitOne();
        }
    }
}