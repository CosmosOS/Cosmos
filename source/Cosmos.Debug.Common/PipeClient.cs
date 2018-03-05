using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Xml;

namespace Cosmos.Debug.Common {
  public class PipeClient {
    private string mPipeName;
    private NamedPipeClientStream mPipe;
    private StreamWriter mWriter;

    public PipeClient(string aPipeName) {
      mPipeName = aPipeName;
    }

    public bool IsConnected
    {
      get
      {
        lock (this)
        {
          return mPipe != null;
        }
      }
    }

    public int Read(byte[] buffer, int offset, int count)
    {
      if (!TryConnect())
      {
        return 0;
      }
      mPipe.ReadTimeout = 25;
      return mPipe.Read(buffer, offset, count);
    }

    public void SendRawToChannel(byte aChannel, byte aCommand, byte[] aData)
    {
      SendCommand((ushort)((aChannel << 8) | aCommand), aData);
    }

    public void SendCommand(ushort aCmd, string aData)
    {
      SendCommand(aCmd, Encoding.UTF8.GetBytes(aData));
    }

    public void SendCommand(ushort aCmd, byte[] aData = null)
    {
      if (!TryConnect())
      {
        return;
      }
      try
      {
        mPipe.WriteByte((byte)(aCmd >> 8));
        mPipe.WriteByte((byte)(aCmd & 0xFF));

        byte[] xData = aData;
        if (xData == null)
        {
            xData = new byte[0];
        }

        //int xLength = Math.Min(xData.Length, 32768);
        int xLength = xData.Length;
        mPipe.WriteByte((byte) (xLength >> 24));
        mPipe.WriteByte((byte) (xLength >> 16));
        mPipe.WriteByte((byte) (xLength >> 8));
        mPipe.WriteByte((byte) (xLength & 0xFF));
        if (xLength > 0)
        {
            mPipe.Write(xData, 0, xLength);
        }
        mPipe.Flush();
      }
      catch
      {
      }
    }

    private bool TryConnect()
    {
      // We need to delay creation and connect until its used, so we guarantee
      // that the server side is active and ready.

      // Because we have a timeout in connect this can happen more than once
      // concurrently if the user is quick. I had this issue several times during
      // testing. To avoid this, we block with a lock.
      lock (this)
      {
        if (mPipe == null|| !mPipe.IsConnected)
        {
          var xPipe = new NamedPipeClientStream(".", mPipeName, PipeDirection.Out);
          try
          {
            // For now we assume its there or not from the first call.
            // If we don't find the server, we disable it to avoid causing lag.
            // TODO: In future - try this instead:
            // String[] listOfPipes = System.IO.Directory.GetFiles(@"\.\pipe\");
            // or maybe not - what we have seems to work just fine...

            xPipe.Connect(500);
          }
          catch (Exception)
          {
            xPipe.Dispose();
            //xPipe.Close();
            return false;
          }
          mWriter = new StreamWriter(xPipe);
          // Only set mPipe if we are truly ready. Other code can check it.
          mPipe = xPipe;
        }
      }
      return true;
    }
  }
}
