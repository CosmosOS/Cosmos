using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;
using System.Threading;
using System.IO.Ports;

using Cosmos.Build.Common;

namespace Cosmos.Debug.Hosts {
  public class Slave : Host {
    string mPortName;
    SerialPort mPort;
    Thread mPowerStateThread;

    public Slave(Dictionary<string, string> aParams, bool aUseGDB)
      : base(aParams, aUseGDB) {
      var xPort = mParams[BuildPropertyNames.SlavePortString];
      if (xPort == "None") {
        throw new Exception("No slave port is set.");
      }

      var xParts = xPort.Split(' ');
      mPortName = xParts[1];
    }

    string WaitForPrompt() {
      var xSB = new StringBuilder();
      char xLastChar = ' ';
      char xChar = ' ';
      while (true) {
        xLastChar = xChar;
        xChar = (char)mPort.ReadChar();
        xSB.Append(xChar);
        if (xChar == ':' && xLastChar == ':') {
          break;
        }
      }
      // Remove ::
      xSB.Length = xSB.Length - 2;
      return xSB.ToString();
    }

    void TogglePowerSwitch() {
      Send("REL4.ON");
      Thread.Sleep(500);
      Send("REL4.OFF");
    }

    bool IsOn() {
      var xResult = Send("CH1.GET").Split('\n');
      return xResult[1][0] == '1';
    }

    string Send(string aData) {
      // Dont use writeline, it only sends /n or /r (didnt bother to find out which, we need both)
      mPort.Write(aData + "\r\n");
      return WaitForPrompt();
    }

    void WaitPowerState(bool aOn) {
      int xCount = 0;
      while (IsOn() == !aOn) {
        Thread.Sleep(250);
        xCount++;
        // 5 seconds
        if (xCount == 20) {
          throw new Exception("Slave did not respond to power command.");
        }
      }
    }

    public override void Start() {
      mPort = new SerialPort(mPortName);
      mPort.Open();

      Send("");
      // Set to digital input
      Send("CH1.SETMODE(2)");

      if (IsOn()) {
        TogglePowerSwitch();
        WaitPowerState(false);
        // Small pause for discharge
        Thread.Sleep(1000);
      }

      TogglePowerSwitch();
      // Give PC some time to turn on, else we will detect it as off right away.
      WaitPowerState(true);

      if (OnShutDown != null) {
        mPowerStateThread = new Thread(delegate() {
          while (true) {
            Thread.Sleep(1000);
            if (!IsOn()) {
              mPort.Close();
              OnShutDown(this, EventArgs.Empty);
              break;
            }
          }
        });
        mPowerStateThread.Start();
      }
    }

    public override void Stop() {
      if (mPowerStateThread != null) {
        mPowerStateThread.Abort();
        mPowerStateThread.Join();
      }

      if (IsOn()) {
        TogglePowerSwitch();
        WaitPowerState(false);
      }
      mPort.Close();
    }
  }
}
