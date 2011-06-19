using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Text;

namespace Cosmos.Debug.VSDebugEngine {

  static public class DebugWindows {
    static private NamedPipeClientStream mPipe;
    static private StreamWriter mWriter;

    static public void SendCommand(byte aCmd, byte[] aData) {
      if (mPipe == null) {
        mPipe = new NamedPipeClientStream(".", "CosmosDebugWindows", PipeDirection.Out);
        mPipe.Connect();
        mWriter = new StreamWriter(mPipe);
      }
      mWriter.Write(aCmd);
      mWriter.Write(aData);
      mPipe.Flush();
    }

  }

}
