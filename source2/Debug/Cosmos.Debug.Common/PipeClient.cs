using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Text;

namespace Cosmos.Debug.Common {
  public class PipeClient {
    private string mPipeName;
    private NamedPipeClientStream mPipe;
    private StreamWriter mWriter;

    public PipeClient(string aPipeName) {
      mPipeName = aPipeName;
    }

    public void SendCommand(byte aCmd, byte[] aData) {
      // We need to delay creation and connect until its used, so we guarantee
      // that the server side is active and ready.
      if (mPipe == null) {
        mPipe = new NamedPipeClientStream(".", Cosmos.Debug.Consts.Pipes.DownName, PipeDirection.Out);
        try {
          // For now we assume its there or not from the first call.
          // If we don't find the server, we disable it to avoid causing lag.
          // TODO: In future - try this instead:
          // String[] listOfPipes = System.IO.Directory.GetFiles(@"\.\pipe\");
          // or maybe not - what we have seems to work just fine...

          mPipe.Connect(500);
        } catch (TimeoutException ex) {
          mPipe.Close();
          mPipe = null;
          return;
        }
        mWriter = new StreamWriter(mPipe);
      }

      mPipe.WriteByte(aCmd);

      byte[] xData = aData;
      if (xData == null) {
        xData = new byte[0];
      }
      int xLength = Math.Min(aData.Length, 32768);
      mPipe.WriteByte((byte)(xLength >> 8));
      mPipe.WriteByte((byte)(xLength & 0xFF));
      if (xLength > 0) {
        mPipe.Write(aData, 0, xLength);
      }

      mPipe.Flush();
    }

  }
}
