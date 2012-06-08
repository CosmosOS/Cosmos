using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;

namespace Cosmos.Build.Installer {
  public class Task {
    public void Start(string aEXE, string aParams) {
      Start(aEXE, aParams, true);
    }
    public void Start(string aExe, string aParams, bool aWait) {
      var xStart = new ProcessStartInfo();
      xStart.FileName = aExe;
      xStart.WorkingDirectory = CurrPath;
      xStart.Arguments = aParams;
      xStart.UseShellExecute = false;
      xStart.RedirectStandardOutput = true;
      //xStart.RedirectStandardError = true;
      using (var xProcess = Process.Start(xStart)) {
        using (var xReader = xProcess.StandardOutput) {
          string xRresult = xReader.ReadToEnd();
        }
      }
    }

    private Log mLog = new Log();
    public Log Log { get { return mLog; } }

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
      File.Copy(Path.Combine(SrcPath, aSrcPathname), Path.Combine(CurrPath, aDestPathname));
    }

    public void Echo() {
      mLog.Echo("");
    }
    public void Echo(string aText) {
      mLog.Echo(aText);
    }
    public void EchoOn() {
      mLog.EchoOn();
    }
    public void EchoOff() {
      mLog.EchoOff();
    }
  }
}
