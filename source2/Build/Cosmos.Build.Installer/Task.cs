using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Cosmos.Build.Installer {
  public class Task {
    public Task() {
      CurrPath = Directory.GetCurrentDirectory();
    }

    private Log mLog = new Log();
    public Log Log { get { return mLog; } }

    public string CurrPath { get; set; }
    //
    private string mSrcPath;
    public string SrcPath {
      get {
        return string.IsNullOrWhiteSpace(mSrcPath) ? CurrPath : mSrcPath;
      }
      set { mSrcPath = value; }
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
