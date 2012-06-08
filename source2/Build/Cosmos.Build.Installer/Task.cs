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
      DoRun();
    }

    public void StartConsole(string aExe, string aParams) {
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
      CurrPath = aPath;
    }

    public void Copy(string aSrcPathname) {
      Copy(aSrcPathname, Path.GetFileName(aSrcPathname));
    }
    public void Copy(string aSrcPathname, string aDestPathname) {
      File.Copy(Path.Combine(SrcPath, aSrcPathname), Path.Combine(CurrPath, aDestPathname), true);
    }

    public void Echo() {
      mLog.WriteLine("");
    }
    public void Echo(string aText) {
      mLog.WriteLine(aText);
    }
  }
}
