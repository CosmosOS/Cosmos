using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Windows.Threading;
using System.Windows;
using Cosmos.Compiler.Debug;

namespace Cosmos.VS.Debug {
  class PipeThread {
    static protected bool KillThread = false;
    static protected NamedPipeServerStream mPipe;
    static public event Action<byte, byte[]> DataPacketReceived;
    protected const string PipeName = @"Cosmos\DebugWindows";

    static public void Stop() {
      PipeThread.KillThread = true;
      if (mPipe.IsConnected) {
        mPipe.Close();
      } else {
        // Kick it out of the WaitForConnection
        var xPipe = new NamedPipeClientStream(".", PipeThread.PipeName, PipeDirection.Out);
        xPipe.Connect(100);
      }
    }

    // See comment in ctor as to why we have to make this new ReadByte replacement
    static byte ReadByte() {
      byte[] xByte = new byte[1];
      if (mPipe.Read(xByte, 0, 1) == 0) {
        throw new Exception("Pipe has been closed.");
      }
      return xByte[0];
    }

    static public void ThreadStartServer() {
      try {
        // Some idiot MS intern must have written the blocking part of pipes. There is no way to
        // cancel WaitForConnection, or ReadByte. (pipes introduced in 3.5, I thought one time I read
        // that 4.0 added an abort option, but I cannot find it)
        // If you set Async as the option, but use sync calls, .Close can kind of kill them.
        // It causes exceptions to be raised in WaitForConnection and ReadByte (possibly due to another
        // issue with threads and exceptions without handlers, maybe check again later), but they just
        // loop over and over on it... READ however with the async option WILL exit with 0....
        // Its like VB1 and adding to sorted listboxes over all again... no one dogfooded this stuff.
        // And yes we could use async.. but its SOOO much messier and far more complicated than it ever
        // should be.
        //
        // Here is an interesting approach using async and polling... If need be we can go that way:
        // http://stackoverflow.com/questions/2700472/how-to-terminate-a-managed-thread-blocked-in-unmanaged-code

        while (!KillThread) { // Loop again to allow mult incoming connections between debug sessions
          using (mPipe = new NamedPipeServerStream(PipeName, PipeDirection.In, 1, PipeTransmissionMode.Message, PipeOptions.Asynchronous)) {
            mPipe.WaitForConnection();

            byte xCmd;
            int xSize;
            while (mPipe.IsConnected && !KillThread) {
              xCmd = ReadByte();

              xSize = ReadByte() << 8;
              xSize = xSize | ReadByte();

              byte[] xMsg = new byte[xSize];
              mPipe.Read(xMsg, 0, xSize);

              DataPacketReceived(xCmd, xMsg);
            }
          }
        }
      } catch { 
        // Threads MUST have an exception handler
        // Otherwise there are side effects when an exception occurs
      } 
    }
  
  }
}
