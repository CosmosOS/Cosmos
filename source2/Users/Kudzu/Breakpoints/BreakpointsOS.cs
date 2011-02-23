using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sys = Cosmos.System;
using Cosmos.Debug.Kernel;
using Cosmos.Common.Extensions;
using Cosmos.Hardware.BlockDevice;

namespace BreakpointsKernel {
  public class BreakpointsOS : Sys.Kernel {
    public BreakpointsOS() {
      ClearScreen = false;
    }

    protected override void BeforeRun() {
      Console.WriteLine("Cosmos boot complete.");
    }

    protected void TestSB() {
      var xSB = new StringBuilder();
      xSB.Append("Hello");
      xSB.Append("Hello");
      var xDisplay = xSB.ToString();
      Console.WriteLine(xDisplay.Length);
      Console.WriteLine(xDisplay);
    }

    protected override void Run() {
      var xATA = (AtaPio)BlockDevice.Devices[0];
      Console.WriteLine("--------------------------");
      Console.WriteLine("Type: " + (xATA.DriveType == AtaPio.SpecLevel.ATA ? "ATA" : "ATAPI"));
      Console.WriteLine("Serial No: " + xATA.SerialNo);
      Console.WriteLine("Firmware Rev: " + xATA.FirmwareRev);
      Console.WriteLine("Model No: " + xATA.ModelNo);
      var x = xATA.BlockCount;
      var y = xATA.BlockSize;
      Console.WriteLine("Size: " + xATA.BlockCount * xATA.BlockSize / 1024 / 1024 + " MB");

      //var xWrite = new byte[512];
      //for (int i = 0; i < 512; i++) {
      //  xWrite[i] = (byte)i;
      //}
      //xATA.WriteBlock(0, xWrite);

      //var xRead = new byte[512];
      //xATA.ReadBlock(0, xRead);
      //string xDisplay = "";
      //for (int i = 0; i < 512; i++) {
      //  xDisplay = xDisplay + xRead[i].ToHex();
      //}
      //Console.WriteLine(xDisplay);
      Stop();
    }

    protected override void AfterRun() {
      Console.Write("Done");
      Console.ReadLine();
    }
  }
}
