using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cosmos.Build.Installer;

namespace Cosmos.Build.Builder {
  public class FileMgr : IDisposable {
    public string SrcPath;
    public string DestPath;

    public FileMgr(string aSrcPath, string aDestPath) {
      SrcPath = aSrcPath;
      DestPath = aDestPath;
    }

    public void ResetReadOnly(string aPathname) {
      var xAttrib = File.GetAttributes(aPathname);
      if ((xAttrib & FileAttributes.ReadOnly) == FileAttributes.ReadOnly) {
        File.SetAttributes(aPathname, xAttrib & ~FileAttributes.ReadOnly);
      }
    }

    public void Copy(string aSrcPathname, bool aClearReadonly = true) {
      Copy(aSrcPathname, Path.GetFileName(aSrcPathname), aClearReadonly);
    }
    public void Copy(string aSrcPathname, string aDestPathname, bool aClearReadonly = true) {
      Log.WriteLine("Copy");

      string xSrc = Path.Combine(SrcPath, aSrcPathname);
      Log.WriteLine("  From: " + xSrc);

      string xDest = Path.Combine(DestPath, aDestPathname);
      Log.WriteLine("  To: " + xDest);

      // Copying files that are in TFS often they will be read only, so need to kill this file before copy
      // We don't use TFS any more.. but left the code.
      if (aClearReadonly && File.Exists(xDest)) {
        ResetReadOnly(xDest);
      }
      File.Copy(xSrc, xDest, true);
      ResetReadOnly(xDest);
    }

    // Dummy pattern to allow scoping via using.
    // Hacky, but for what we are doing its fine and the GC
    // effects are negligible in our usage.
    protected virtual void Dispose(bool aDisposing) {
    }

    public void Dispose() {
      Dispose(true);
    }

  }
}
