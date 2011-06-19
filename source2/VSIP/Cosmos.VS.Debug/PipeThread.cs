using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Windows.Threading;

namespace Cosmos.VS.Debug {
  class PipeThread {
    public static event Action<string> DataPacketReceived;

    public static void ThreadStartServer() {
      using (var xPipe = new NamedPipeServerStream("CosmosDebugWindows", PipeDirection.In)) {
        xPipe.WaitForConnection();
        using (var xReader = new StreamReader(xPipe)) {
          while (!xReader.EndOfStream) {
            string xLine = xReader.ReadLine();

            Dispatcher.CurrentDispatcher.Invoke(DispatcherPriority.Normal,
              (Action)delegate() {
                if (DataPacketReceived != null) {
                  DataPacketReceived(xLine);
                }
              }
            );
          }
        }
      }
    }
  
  }
}
