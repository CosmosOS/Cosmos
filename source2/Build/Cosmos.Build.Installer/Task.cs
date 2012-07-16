using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;
using System.Threading;

namespace Cosmos.Build.Installer {
  public abstract class Task {
    protected abstract void DoRun();

    public void Run() {
      try {
        DoRun();
      } catch (Exception ex) {
        Log.NewSection("Error");
        Log.WriteLine(ex.Message);
        Log.SetError();
      }
    }

    public bool AmRunning32Bit() {
      return IntPtr.Size == 4;
    }

    public bool IsRunning(string aName) {
      var xList = Process.GetProcessesByName(aName);
      return xList.Length > 0;
    }

    public void WaitForStart(string aName) {
      WaitForStart(aName, null);
    }
    public bool WaitForStart(string aName, int? aMilliSec) {
      return WaitForState(aName, aMilliSec, true);
    }
    public void WaitForExit(string aName) {
      WaitForExit(aName, null);
    }
    public bool WaitForExit(string aName, int? aMilliSec) {
      return WaitForState(aName, aMilliSec, false);
    }
    public bool WaitForState(string aName, int? aMilliSec, bool aIsRunning) {
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
          throw new Exception("Console returned exit code. (" + xProcess.ExitCode + ")");
        }
      }
    }

    public void Start(string aExe, string aParams) {
      Start(aExe, aParams, true, true);
    }
    public void Start(string aExe, string aParams, bool aWait, bool aShowWindow) {
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
            throw new Exception("Application returned exit code.");
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

    public void Copy(string aSrcPathname) {
      Copy(aSrcPathname, Path.GetFileName(aSrcPathname));
    }
    public void Copy(string aSrcPathname, string aDestPathname) {
      Log.WriteLine("Copy");

      string xSrc = Path.Combine(SrcPath, aSrcPathname);
      Log.WriteLine("  From: " + xSrc);

      string xDest = Path.Combine(CurrPath, aDestPathname);
      Log.WriteLine("  To: " + xDest);

      // TODO: Make overwrite a param and make this part of the logic
      // Copying files that are in TFS often they will be read only, so need to kill this file before copy
      if (File.Exists(xDest)) {
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
