using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO.Ports;
using System.Threading;

namespace Cosmos.Launch.Slave {
  public class DebugHost : Cosmos.Launch.Common.DebugHost {
    string mPortName;
    SerialPort mPort;
    bool mDetailedLogging = false;

    public DebugHost(string[] aArgs) : base(aArgs, 1) {
      mPortName = aArgs[0];
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
      if (mDetailedLogging) {
        Console.WriteLine("Recv: " + xSB.ToString());
      }
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
      if (mDetailedLogging) {
        Console.WriteLine("Sent: " + aData);
      }
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

    protected override int Run() {
      Console.WriteLine("Opening " + mPortName);
      mPort = new SerialPort(mPortName);
      mPort.Open(); try {
        Console.WriteLine("Initializing Canakit UK1104");
        Send("");
        // Set to digital input
        Send("CH1.SETMODE(2)");

        if (IsOn()) {
          Console.WriteLine("Slave is on. Powering off.");
          TogglePowerSwitch();
          WaitPowerState(false);
          // Small pause for discharge
          Thread.Sleep(1000);
        }

        Console.WriteLine("Powering on.");
        TogglePowerSwitch();
        // Give PC some time to turn on, else we will detect it as off right away.
        WaitPowerState(true);

        Console.WriteLine("Monitoring power status.");
        while (IsOn()) {
          Thread.Sleep(250);
          string xLine = CheckInputLine();
          if (xLine == null) {
          } else if (string.Equals(xLine, "off", StringComparison.InvariantCultureIgnoreCase)) {
            Console.WriteLine("Powering off.");
            TogglePowerSwitch();
            WaitPowerState(false);
          }
        }
        Console.WriteLine("Power is off. Exiting.");
      } finally {
        Console.WriteLine("Closing port.");
        mPort.Close();
      }

      Console.ReadLine();

      return 0;
    }
  }
}
