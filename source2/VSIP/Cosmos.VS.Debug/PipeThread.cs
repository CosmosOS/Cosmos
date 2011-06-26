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
    static protected bool KillThread = false;
    static protected NamedPipeServerStream mPipe;
    static public event Action<byte, byte[]> DataPacketReceived;
    protected const string PipeName = "CosmosDebugWindows";

    static public void Stop() {
      if (!mPipe.IsConnected) {
        PipeThread.KillThread = true;
        // Kick it out of the WaitForConnection
        var xPipe = new NamedPipeClientStream(".", PipeThread.PipeName, PipeDirection.Out);
        xPipe.Connect(100);
      } else {
        mPipe.Close();
      }
    }

    static public void ThreadStartServer() {
      mPipe = new NamedPipeServerStream(PipeName, PipeDirection.In, 1, PipeTransmissionMode.Message, PipeOptions.None);
      mPipe.WaitForConnection();
      if (KillThread) {
        return;
      }

      using (var xReader = new StreamReader(mPipe)) {
        while ((mPipe.IsConnected) && (!KillThread)) {
          byte xMsgType = 0;
          byte[] xMsg = new byte[255];
          xMsgType = (byte)mPipe.ReadByte();
          mPipe.Read(xMsg, 0, 255);
          DataPacketReceived(xMsgType, xMsg);
        }
        mPipe.Disconnect();
      }
    }
  
  }
}
