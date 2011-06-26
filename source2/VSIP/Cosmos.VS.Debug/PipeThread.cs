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
    public static bool Connected = false;
    public static event Action<byte, byte[]> DataPacketReceived;
    public const string PipeName = "CosmosDebugWindows";

    public static void ThreadStartServer() {
      using (var xPipe = new NamedPipeServerStream(PipeName, PipeDirection.In, 1, PipeTransmissionMode.Message, PipeOptions.None)) {
        xPipe.WaitForConnection();
        if (KillThread) {
          return;
        }
        Connected = true;

        using (var xReader = new StreamReader(xPipe)) {
          while ((xPipe.IsConnected) || (!KillThread)) {
            byte xMsgType = 0;
            byte[] xMsg = new byte[255];
            xMsgType = (byte)xPipe.ReadByte();
            xPipe.Read(xMsg, 0, 255);
            DataPacketReceived(xMsgType, xMsg);
          }
          xPipe.Disconnect();
        }
      }
    }
  
  }
}
