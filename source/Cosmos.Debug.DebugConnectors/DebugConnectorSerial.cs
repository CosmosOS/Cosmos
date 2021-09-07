using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;

namespace Cosmos.Debug.DebugConnectors {
  /// <summary>Use a serial port to implement wire transfer protocol between a Debug Stub hosted
  /// in a debugged Cosmos Kernel and our Debug Engine hosted in Visual Studio.</summary>
  public class DebugConnectorSerial : DebugConnectorStreamWithTimeouts {
    private SerialPort mPort;

    public DebugConnectorSerial(string aPort) {
      DebugLog("Connecting to port " + aPort);
      mPort = new SerialPort(aPort);
      mPort.BaudRate = 115200;
      mPort.Parity = Parity.None;
      mPort.DataBits = 8;
      mPort.StopBits = StopBits.One;
      Start();
    }

    protected override void InitializeBackground()
    {
      DebugLog("Try opening port now");
      try {
        mPort.Open();
      } catch (Exception E) {
        DebugLog("Error opening serial port: " + E.ToString());
      }

      mStream = mPort.BaseStream;
    }

    protected override bool GetIsConnectedToDebugStub()
    {
        return mPort.IsOpen;
    }
  }
}
