using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.IO.Pipes;
using System.Threading;
using System.Diagnostics;

namespace Cosmos.Launch.VMware {
  public class DebugHost : Cosmos.Launch.Common.DebugHost {
    public DebugHost(string[] aArgs)
      : base(aArgs) {
    }

    protected override int Run() {
      // Arg[0] - ShellExecute
      bool xShellExecute = (string.Compare(mArgs[0], "true", true) == 0);

      // Target exe or file
      var xStartInfo = new ProcessStartInfo();
      xStartInfo.FileName = mArgs[1];
      var xArgSB = new StringBuilder();

      // Skip other arguments and "rearg" the rest
      foreach (var arg in mArgs.Skip(2)) {
        xArgSB.AppendFormat("\"{0}\" ", arg);
      }
      xStartInfo.Arguments = xArgSB.ToString();
      xStartInfo.RedirectStandardError = !xShellExecute;
      xStartInfo.RedirectStandardOutput = !xShellExecute;
      xStartInfo.UseShellExecute = xShellExecute;
      var xProcess = Process.Start(xStartInfo);

      xProcess.WaitForExit();
      if (!xShellExecute) {
        Console.WriteLine(xProcess.StandardError.ReadToEnd());
        Console.WriteLine(xProcess.StandardOutput.ReadToEnd());
      }
      return xProcess.ExitCode;
    }
  }
}
