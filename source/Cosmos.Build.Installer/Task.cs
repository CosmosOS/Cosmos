using System;
using System.IO;
using System.Diagnostics;
using System.Threading;
using System.Collections.Generic;
using System.Linq;

namespace Cosmos.Build.Installer {
  public abstract class Task {
    protected abstract List<string> DoRun();

    public void Run() {
      var exceptions = new List<string>();
      try
      {
          exceptions.AddRange(DoRun());
      }
      catch(Exception ex){
        exceptions.Add(ex.Message);
        if (ex.InnerException != null){
            exceptions.Add(ex.InnerException.Message);
        }
        exceptions.Add(ex.StackTrace);
      }

      if (exceptions.Any()) {
        Log.SetError();
        Log.NewSection("Error");
        //Collect all the exceptions from the build stage, and list them
        foreach(var msg in exceptions) {
          Log.WriteLine(msg);
        }
      }
    }

    public bool AmRunning32Bit() {
      return IntPtr.Size == 4;
    }

    public bool IsRunning(string aName) {
      var xList = Process.GetProcessesByName(aName);
      return xList.Length > 0;
    }

    public bool WaitForStart(string aName, int? aMilliSec = null) {
      return WaitForState(aName, true, aMilliSec);
    }
    public bool WaitForExit(string aName, int? aMilliSec = null) {
      return WaitForState(aName, false, aMilliSec);
    }
    public bool WaitForState(string aName, bool aIsRunning, int? aMilliSec) {
      while (IsRunning(aName) != aIsRunning) {
        Thread.Sleep(200);
        if (aMilliSec.HasValue) {
          aMilliSec = aMilliSec - 200;
          if (aMilliSec <= 0) {
            return true;
          }
        }
      }
      return false;
    }

    public void StartConsole(string aExe, string aParams) {
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
      using (var xProcess = Process.Start(xStart)) {
        using (var xReader = xProcess.StandardOutput) {
          string xLine;
          while (true) {
            xLine = xReader.ReadLine();
            if (xLine == null) {
              break;
            }
            Log.WriteLine(xLine);
          }
        }
        xProcess.WaitForExit();
        if (xProcess.ExitCode != 0) {
          Log.SetError();
          Log.WriteLine(xProcess.StandardError.ReadToEnd());
          throw new Exception("Console returned exit code. (0x" + xProcess.ExitCode.ToString("X8") + ")");
        }
      }
    }

    public void Start(string aExe, string aParams, bool aWait = true, bool aShowWindow = true) {
      Log.WriteLine("Starting: " + aExe);
      Log.WriteLine("  Params: " + aParams);

      using (var xProcess = new Process()) {
        var xPSI = xProcess.StartInfo;
        xPSI.FileName = aExe;
        xPSI.WorkingDirectory = CurrPath;
        xPSI.Arguments = aParams;
        xPSI.UseShellExecute = false;
        xPSI.CreateNoWindow = !aShowWindow;
        xProcess.Start();
        if (aWait) {
          xProcess.WaitForExit();
          if (xProcess.ExitCode != 0) {
            Log.SetError();
            throw new ApplicationException("Application returned exit code. (0x" + xProcess.ExitCode.ToString("X8") + ")");
          }
        }
      }
    }

    private Log mLog = new Log();
    public Log Log { get { return mLog; } }

    public void Section(string aText) {
      Log.NewSection(aText);
    }

    public string CurrPath {
      get { return Directory.GetCurrentDirectory(); }
      set { Directory.SetCurrentDirectory(value); }
    }
    //
    private string mSrcPath;
    public string SrcPath {
      get {
        return string.IsNullOrWhiteSpace(mSrcPath) ? CurrPath : mSrcPath;
      }
      set { mSrcPath = value; }
    }

    public string Quoted(string aValue) {
      return "\"" + aValue + "\"";
    }

    public void CD(string aPath) {
      ChDir(aPath);
    }
    public void ChDir(string aPath) {
      Log.WriteLine("Change Dir: " + aPath);
      CurrPath = aPath;
    }

    public void ResetReadOnly(string aPathname) {
      var xAttrib = File.GetAttributes(aPathname);
      if ((xAttrib & FileAttributes.ReadOnly) == FileAttributes.ReadOnly) {
        File.SetAttributes(aPathname, xAttrib & ~FileAttributes.ReadOnly);
      }
    }

    public void Copy(string aSrcPathname, bool clearReadonlyIfDestExists) {
      Copy(aSrcPathname, Path.GetFileName(aSrcPathname), clearReadonlyIfDestExists);
    }
    public void Copy(string aSrcPathname, string aDestPathname, bool clearReadonlyIfDestExists) {
      Log.WriteLine("Copy");

      string xSrc = Path.Combine(SrcPath, aSrcPathname);
      Log.WriteLine("  From: " + xSrc);

      string xDest = Path.Combine(CurrPath, aDestPathname);
      Log.WriteLine("  To: " + xDest);

      // Copying files that are in TFS often they will be read only, so need to kill this file before copy
      if (clearReadonlyIfDestExists && File.Exists(xDest)) {
        ResetReadOnly(xDest);
      }
      File.Copy(xSrc, xDest, true);
      ResetReadOnly(xDest);
    }

    public void Echo() {
      Echo("");
    }
    public void Echo(string aText) {
      mLog.WriteLine(aText);
    }
  }
}
