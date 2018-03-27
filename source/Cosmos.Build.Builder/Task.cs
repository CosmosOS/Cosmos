using System;
using System.IO;
using System.Diagnostics;
using System.Threading;
using System.Collections.Generic;
using System.Linq;

namespace Cosmos.Build.Builder.Tasks
{
    internal abstract class Task
    {
        protected abstract List<string> DoRun();

        protected ILogger Logger { get; }

        protected Task(ILogger logger)
        {
            Logger = logger;
        }

        public void Run()
        {
            var exceptions = new List<string>();
            try
            {
                exceptions.AddRange(DoRun());
            }
            catch (Exception ex)
            {
                exceptions.Add(ex.Message);
                if (ex.InnerException != null)
                {
                    exceptions.Add(ex.InnerException.Message);
                }
                exceptions.Add(ex.StackTrace);
            }

            if (exceptions.Any())
            {
                Logger.SetError(); // Existing section

                Logger.NewSection("Error");
                Logger.SetError();
                //Collect all the exceptions from the build stage, and list them
                foreach (var msg in exceptions)
                {
                    Logger.LogMessage(msg);
                }
            }
        }

        public bool IsRunning(string aName)
        {
            var xList = Process.GetProcessesByName(aName);
            return xList.Length > 0;
        }

        public void KillProcesses(string aName)
        {
            foreach (var p in Process.GetProcessesByName(aName))
            {
                p.Kill();
            }
        }

        public bool WaitForStart(string aName, int? aMilliSec = null)
        {
            return WaitForState(aName, true, aMilliSec);
        }
        public bool WaitForExit(string aName, int? aMilliSec = null)
        {
            return WaitForState(aName, false, aMilliSec);
        }
        public bool WaitForState(string aName, bool aIsRunning, int? aMilliSec)
        {
            while (IsRunning(aName) != aIsRunning)
            {
                Thread.Sleep(200);
                if (aMilliSec.HasValue)
                {
                    aMilliSec = aMilliSec - 200;
                    if (aMilliSec <= 0)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public void StartConsole(string aExe, string aParams)
        {
            Logger.LogMessage("Starting: " + aExe);
            Logger.LogMessage("  Params: " + aParams);

            var xStart = new ProcessStartInfo();
            xStart.FileName = aExe;
            xStart.WorkingDirectory = Directory.GetCurrentDirectory();
            xStart.Arguments = aParams;
            xStart.UseShellExecute = false;
            xStart.CreateNoWindow = true;
            xStart.RedirectStandardOutput = true;
            xStart.RedirectStandardError = true;
            using (var xProcess = Process.Start(xStart))
            {
                using (var xReader = xProcess.StandardOutput)
                {
                    string xLine;
                    while (true)
                    {
                        xLine = xReader.ReadLine();
                        if (xLine == null)
                        {
                            break;
                        }
                        Logger.LogMessage(xLine);
                    }
                }
                xProcess.WaitForExit();
                if (xProcess.ExitCode != 0)
                {
                    Logger.SetError();
                    Logger.LogMessage(xProcess.StandardError.ReadToEnd());
                    throw new Exception("Console returned exit code. (0x" + xProcess.ExitCode.ToString("X8") + ")");
                }
            }
        }

        public void Start(string aExe, string aParams, bool aWait = true, bool aShowWindow = true)
        {
            Logger.LogMessage("Starting: " + aExe);
            Logger.LogMessage("  Params: " + aParams);

            using (var xProcess = new Process())
            {
                var xPSI = xProcess.StartInfo;
                xPSI.FileName = aExe;
                xPSI.WorkingDirectory = Directory.GetCurrentDirectory();
                xPSI.Arguments = aParams;
                xPSI.UseShellExecute = false;
                xPSI.CreateNoWindow = !aShowWindow;
                xProcess.Start();
                if (aWait)
                {
                    xProcess.WaitForExit();
                    if (xProcess.ExitCode != 0)
                    {
                        Logger.SetError();
                        throw new ApplicationException("Application returned exit code. (0x" + xProcess.ExitCode.ToString("X8") + ")");
                    }
                }
            }
        }

        public void Section(string aText)
        {
            Logger.NewSection(aText);
        }

        //
        private string mSrcPath;

        public string Quoted(string aValue)
        {
            return "\"" + aValue + "\"";
        }

    }
}
