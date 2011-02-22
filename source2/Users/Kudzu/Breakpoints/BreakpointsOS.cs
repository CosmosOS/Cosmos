using System;
using System.Collections.Generic;
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
      var xATA = new AtaPio(Cosmos.Core.Global.BaseIOGroups.ATA1, Ata.BusPositionEnum.Master);
      
      var xWrite = new byte[512];
      for (int i = 0; i < 512; i++) {
        xWrite[i] = (byte)i;
      }
      xATA.WriteSector(0, xWrite);

      var xRead = new byte[512];
      xATA.ReadSector(0, xRead);
      string xDisplay = "";
      for (int i = 0; i < 512; i++) {
        xDisplay = xDisplay + xRead[i].ToHex();
      }
      Console.WriteLine(xDisplay);
      Stop();
    }

    protected override void AfterRun() {
      Console.Write("Done");
      Console.ReadLine();
    }
  }
}
