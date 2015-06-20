using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Remoting.Channels;
using System.Security;

namespace Cosmos.TestRunner.Core
{
    partial class Engine
    {
        private void RunProcess(string fileName, string workingDirectory, string[] arguments)
        {
            if (arguments == null)
            {
                throw new ArgumentNullException("arguments");
            }
            var xStartInfo = new ProcessStartInfo(fileName);
            xStartInfo.WorkingDirectory = workingDirectory;
            xStartInfo.Arguments = arguments.Aggregate("", (a, b) => a + " \"" + b + "\"");
            xStartInfo.RedirectStandardError = true;
            xStartInfo.RedirectStandardOutput = true;
            xStartInfo.UseShellExecute = false;

            var xProcess = new Process();
            xProcess.StartInfo = xStartInfo;

            xProcess.OutputDataReceived += (sender, e) => DoLog(e.Data);
            xProcess.ErrorDataReceived += (sender, e) => DoLog(e.Data);
            xProcess.Start();
            xProcess.BeginErrorReadLine();
            xProcess.BeginOutputReadLine();
            xProcess.WaitForExit(30000); // max 30 seconds
            if (xProcess.ExitCode != 0)
            {
                throw new Exception("Error running process!");
            }
        }
    }
}
