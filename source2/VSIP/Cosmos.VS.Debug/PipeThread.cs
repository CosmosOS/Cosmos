using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Windows.Threading;
using System.Windows;

namespace Cosmos.VS.Debug {
  class PipeThread {
    public static bool KillThread = false;
    public static event Action<byte, byte[]> DataPacketReceived;

    public static void ThreadStartServer()
    {
      using (var xPipe = new NamedPipeServerStream("CosmosDebugWindows", PipeDirection.In))
      {
        xPipe.WaitForConnection();
        while (!KillThread)
        {
            using (var xReader = new StreamReader(xPipe))
            {
                while (!xReader.EndOfStream)
                {
                    string xLine = xReader.ReadLine();
                    Dispatcher.CurrentDispatcher.Invoke(DispatcherPriority.Normal, (Action)delegate()
                      {
                          if (DataPacketReceived != null)
                          {
                              DataPacketReceived((byte)1, new byte[10] { (byte)1, (byte)1, (byte)1, (byte)1, (byte)1, (byte)1, (byte)1, (byte)1, (byte)1, (byte)1 });
                          }
                      }
                    );
                }
            }
        }
      }
    }
  
  }
}
