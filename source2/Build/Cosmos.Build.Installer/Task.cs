using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;

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
        }
      }
    }

    public void Start(string aExe, string aParams) {
      Start(aExe, aParams, true);
    }
    public void Start(string aExe, string aParams, bool aWait) {
      Log.WriteLine("Starting: " + aExe);
      Log.WriteLine("  Params: " + aParams);

      var xStart = new ProcessStartInfo();
      xStart.FileName = aExe;
      xStart.WorkingDirectory = CurrPath;
      xStart.Arguments = aParams;
      xStart.UseShellExecute = false;
      using (var xProcess = Process.Start(xStart)) {
        if (aWait) {
          xProcess.WaitForExit();
          if (xProcess.ExitCode != 0) {
            Log.SetError();
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

    public void Copy(string aSrcPathname) {
      Copy(aSrcPathname, Path.GetFileName(aSrcPathname));
    }
    public void Copy(string aSrcPathname, string aDestPathname) {
      Log.WriteLine("Copy");

      string xSrc = Path.Combine(SrcPath, aSrcPathname);
      Log.WriteLine("  From: " + xSrc);

      string xDest = Path.Combine(CurrPath, aDestPathname);
      Log.WriteLine("  To: " + xDest);

      File.Copy(xSrc, xDest, true);
    }

    public void Echo() {
      Echo("");
    }
    public void Echo(string aText) {
      mLog.WriteLine(aText);
    }
  }
}
