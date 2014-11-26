using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading;

namespace Cosmos.Debug.Common {
  /// <summary>Use a named pipe server to implement wire transfer protocol between a Debug Stub
  /// hosted in a debugged Cosmos Kernel and our Debug Engine hosted in Visual Studio.
  /// Both VMware and Bochs use a pipe to expose guest serial ports to the host.</summary>
  public class DebugConnectorPipeServer : DebugConnectorStream {
    // private AutoResetEvent mWaitConnectEvent = new AutoResetEvent(false);
    private NamedPipeServerStream mPipe;

    public DebugConnectorPipeServer(string aName) {
      mPipe = new NamedPipeServerStream(aName, PipeDirection.InOut, 1, PipeTransmissionMode.Byte,
        PipeOptions.Asynchronous);
      mPipe.BeginWaitForConnection(new AsyncCallback(DoWaitForConnection), mPipe);
    }

    public void DoWaitForConnection(IAsyncResult aResult) {
      var xPipe = (NamedPipeServerStream)aResult.AsyncState;
      xPipe.EndWaitForConnection(aResult);
      // mWaitConnectEvent.Set();
      Start(xPipe);
    }
  }
}