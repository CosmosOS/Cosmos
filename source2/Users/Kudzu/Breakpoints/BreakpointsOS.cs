using System;
using System.Collections.Generic;
using System.Text;
using Sys = Cosmos.System;
using Cosmos.Debug.Kernel;
using Cosmos.Common.Extensions;

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
      var xATA = new Cosmos.Hardware.BlockDevice.AtaPio(Cosmos.Core.Global.BaseIOGroups.ATA1);
      xATA.Test();

      Stop();
    }

    protected override void AfterRun() {
      Console.Write("Done");
      Console.ReadLine();
    }
  }
}
