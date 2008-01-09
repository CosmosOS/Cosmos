using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.Build.Windows {
    class Global {
        public static void Call(string aEXEPathname, string aArgLine, string aWorkDir) {
            Call(aEXEPathname, aArgLine, aWorkDir, true, true);
        }

        public static void Call(string aEXEPathname, string aArgLine, string aWorkDir, bool aWait, bool aCapture) {
            var xStartInfo = new ProcessStartInfo();
            xStartInfo.FileName = aEXEPathname;
            xStartInfo.Arguments = aArgLine;
            xStartInfo.WorkingDirectory = aWorkDir;
            xStartInfo.CreateNoWindow = false;
            xStartInfo.UseShellExecute = !aCapture;
            xStartInfo.RedirectStandardError = aCapture;
            xStartInfo.RedirectStandardOutput = aCapture;
            var xProcess = Process.Start(xStartInfo);
            if (aWait) {
                if (!xProcess.WaitForExit(60 * 1000) || xProcess.ExitCode != 0) {
                    //TODO: Fix
                    if (aCapture) {
                        Console.WriteLine("Error during call");
                        Console.Write(xProcess.StandardOutput.ReadToEnd());
                        Console.Write(xProcess.StandardError.ReadToEnd());
                    } else {
                        throw new Exception("Call failed");
                    }
                }
            }
        }

    }
}
