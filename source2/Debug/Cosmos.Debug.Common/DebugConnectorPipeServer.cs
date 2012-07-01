using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading;

namespace Cosmos.Debug.Common {
  // Used for Vmware. VMWare uses a pipe to expose guest serial ports to the host.
  public class DebugConnectorPipeServer : DebugConnectorStream {
    private AutoResetEvent mWaitConnectEvent = new AutoResetEvent(false);
    private NamedPipeServerStream mPipe;

    public DebugConnectorPipeServer(string aName) {
      mPipe = new NamedPipeServerStream(aName, PipeDirection.InOut, 1
       , PipeTransmissionMode.Byte, PipeOptions.Asynchronous);
      mPipe.BeginWaitForConnection(new AsyncCallback(DoWaitForConnection), mPipe);
    }

    public void DoWaitForConnection(IAsyncResult aResult) {
      var xPipe = (NamedPipeServerStream)aResult.AsyncState;
      xPipe.EndWaitForConnection(aResult);
      mWaitConnectEvent.Set();
      Start(xPipe);
    }

  }
}