using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Build.Utilities;
using System.Diagnostics;

namespace Cosmos.Build.MSBuild
{
    public abstract class BaseToolTask: Task
    {
        protected bool ExecuteTool(string workingDir, string filename, string arguments, string name)
        {
            var xProcessStartInfo = new ProcessStartInfo();
            xProcessStartInfo.WorkingDirectory = workingDir;
            xProcessStartInfo.FileName = filename;
            xProcessStartInfo.Arguments = arguments;
            xProcessStartInfo.UseShellExecute = false;
            xProcessStartInfo.RedirectStandardOutput = true;
            xProcessStartInfo.RedirectStandardError = true;
            xProcessStartInfo.CreateNoWindow = true;
            using (var xProcess = Process.Start(xProcessStartInfo))
            {
                xProcess.WaitForExit();
                if (xProcess.ExitCode != 0)
                {
                    if (!xProcess.HasExited)
                    {
                        xProcess.Kill();
                        Log.LogError("{0} timed out.", name);
                    }
                    else
                    {
                        Log.LogError("Error occurred while invoking {0}", name);
                    }
                    while (!xProcess.StandardOutput.EndOfStream)
                    {
                        Log.LogMessage("{0} output: {1}", name, xProcess.StandardOutput.ReadLine());
                    }
                    while (!xProcess.StandardError.EndOfStream)
                    {
                        Log.LogMessage("{0} error: {1}", name, xProcess.StandardError.ReadLine());
                    }
                    return false;
                }
            }
            return true;
        }
    }
}
