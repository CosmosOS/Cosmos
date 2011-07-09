using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Cosmos.Debug.VSDebugEngine {

  static public class DebugWindows {
    static private NamedPipeClientStream mPipe;
    static private StreamWriter mWriter;

    static public void SendCommand(byte aCmd, byte[] aData) {
      if (mPipe == null) {
        // User might run mult instances of VS, so we need to make sure the pipe name
        // is unique but also predictable since the pipe is the only way to talk
        // between the debugger and ToolWindows project.
        int xPID = System.Diagnostics.Process.GetCurrentProcess().Id;
        mPipe = new NamedPipeClientStream(".", @"Cosmos\DebugWindows-" + xPID.ToString(), PipeDirection.Out);
        try {
          // For now we assume its there or not from the first call.
          // If we don't find the server, we disable it to avoid causing lag.
          // TODO: In future - try this instead:
          // String[] listOfPipes = System.IO.Directory.GetFiles(@"\.\pipe\");

          mPipe.Connect(500);
        } catch (TimeoutException ex) {
          mPipe.Close();
          mPipe = null;
          return;
        }
        mWriter = new StreamWriter(mPipe);
      }

      mPipe.WriteByte(aCmd);

      int xLength = Math.Min(aData.Length, 32000);
      mPipe.WriteByte((byte)(xLength >> 8));
      mPipe.WriteByte((byte)(xLength & 0xFF));
      if (xLength > 0) {
        mPipe.Write(aData, 0, xLength);
      }

      mPipe.Flush();
    }

  }

}
