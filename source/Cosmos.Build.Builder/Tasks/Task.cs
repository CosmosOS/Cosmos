using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;

namespace Cosmos.Build.Builder.Tasks
{
  public abstract class Task
  {
    protected readonly string mCosmosPath; // Root Cosmos dir
    protected readonly string mVsipPath; // Build/VSIP
    protected readonly string mAppDataPath; // User Kit in AppData
    protected readonly string mSourcePath; // Cosmos source rood
    private readonly List<string> mExceptionList = new List<string>();

    protected Task(string aCosmosDir)
    {
      mCosmosPath = aCosmosDir;
      mVsipPath = Path.Combine(mCosmosPath, @"Build\VSIP");
      mSourcePath = Path.Combine(mCosmosPath, "source");
      mAppDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Cosmos User Kit");
    }

    public void Run()
    {
      try
      {
        DoRun();
      }
      catch (Exception ex)
      {
        AddExceptionMessage(ex);
      }

      if (mExceptionList.Any())
      {
        Log.SetError(); // Existing section

        Log.NewSection("Error");
        Log.SetError();
        //Collect all the exceptions from the build stage, and list them
        foreach (var msg in mExceptionList)
        {
          Log.WriteLine(msg);
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
      Log.WriteLine("Starting: " + aExe);
      Log.WriteLine("  Params: " + aParams);

      var xStart = new ProcessStartInfo();
      xStart.FileName = aExe;
      xStart.WorkingDirectory = CurrPath;
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
            Log.WriteLine(xLine);
          }
        }
        xProcess.WaitForExit();
        if (xProcess.ExitCode != 0)
        {
          Log.SetError();
          Log.WriteLine(xProcess.StandardError.ReadToEnd());
          throw new Exception("Console returned exit code. (0x" + xProcess.ExitCode.ToString("X8") + ")");
        }
      }
    }

    public void Start(string aExe, string aParams, bool aWait = true, bool aShowWindow = true)
    {
      Log.WriteLine("Starting: " + aExe);
      Log.WriteLine("  Params: " + aParams);

      using (var xProcess = new Process())
      {
        var xPSI = xProcess.StartInfo;
        xPSI.FileName = aExe;
        xPSI.WorkingDirectory = CurrPath;
        xPSI.Arguments = aParams;
        xPSI.UseShellExecute = false;
        xPSI.CreateNoWindow = !aShowWindow;
        xProcess.Start();
        if (aWait)
        {
          xProcess.WaitForExit();
          if (xProcess.ExitCode != 0)
          {
            Log.SetError();
            throw new ApplicationException("Application returned exit code. (0x" + xProcess.ExitCode.ToString("X8") + ")");
          }
        }
      }
    }

    public void Section(string aText)
    {
      Log.NewSection(aText);
    }

    public string CurrPath
    {
      get { return Directory.GetCurrentDirectory(); }
      set { Directory.SetCurrentDirectory(value); }
    }

    public string Quoted(string aValue)
    {
      return "\"" + aValue + "\"";
    }

    protected void AddExceptionMessage(Exception ex)
    {
      AddExceptionMessage(ex.Message);
      if (ex.InnerException != null)
      {
        AddExceptionMessage(ex.InnerException.Message);
      }
      AddExceptionMessage(ex.StackTrace);
    }

    protected void AddExceptionMessage(string message)
    {
      mExceptionList.Add(message);
    }

    protected abstract void DoRun();
  }
}
