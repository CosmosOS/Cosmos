using System;
using System.IO.Pipes;
using System.Threading;

namespace Cosmos.Debug.Common {
  public class PipeServer {
    protected bool KillThread = false;
    protected NamedPipeServerStream mPipe;
    public event Action<ushort, byte[]> DataPacketReceived;
    protected string mPipeName;

    public PipeServer(string aPipeName) {
      mPipeName = aPipeName;
    }

    public void Stop() {
      KillThread = true;
      if (mPipe.IsConnected) {
        mPipe.Dispose();
        //mPipe.Close();
      } else {
        // Kick it out of the WaitForConnection
        var xPipe = new NamedPipeClientStream(".", mPipeName, PipeDirection.Out);
        xPipe.Connect(100);
      }
    }

    public void CleanHandlers()
    {
        DataPacketReceived = null;
    }

    // See comment in ctor as to why we have to make this new ReadByte replacement
    protected byte ReadByte() {
      byte[] xByte = new byte[1];
      if (mPipe.Read(xByte, 0, 1) == 0) {
        throw new Exception("Pipe has been closed.");
      }
      return xByte[0];
    }

    protected Thread mThread;
    public void Start() {
      mThread = new Thread(ThreadStartServer);
      mThread.Start();
    }

    protected void ThreadStartServer() {
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

      while (!KillThread)
      {
        // Loop again to allow mult incoming connections between debug sessions
        try
        {
          using (mPipe = new NamedPipeServerStream(mPipeName, PipeDirection.In, 1, PipeTransmissionMode.Message, PipeOptions.Asynchronous)) {
            mPipe.WaitForConnection();

            ushort xCmd;
            int xSize;
            while (mPipe.IsConnected && !KillThread) {
              xCmd = (ushort)(ReadByte() << 8);
              xCmd |= ReadByte();

              xSize = ReadByte() << 24;
              xSize = xSize | ReadByte() << 16;
              xSize = xSize | ReadByte() << 8;
              xSize = xSize | ReadByte();

              byte[] xMsg = new byte[xSize];
              mPipe.Read(xMsg, 0, xSize);

              if (DataPacketReceived != null)
              {
                  DataPacketReceived(xCmd, xMsg);
              }
            }
          }
        } catch (Exception) {
          // Threads MUST have an exception handler
          // Otherwise there are side effects when an exception occurs
        }
      }
    }

  }
}
