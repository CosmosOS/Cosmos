using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;

namespace Cosmos.Build.Installer {
  public class FileCopy {
    public string Dest { get; set; }
    public string Src { get; set; }
    public bool Overwrite { get; set; }

    public FileCopy() {
      Src = Paths.Current;
      Dest = Paths.Current;
      Overwrite = false;
    }

    public void Copy(string aSrcPathname) {
      Copy(aSrcPathname, Path.GetFileName(aSrcPathname));
    }

    public void Copy(string aSrcPathname, string aDestPathname) {
      File.Copy(Path.Combine(Src, aSrcPathname), Path.Combine(Dest, aDestPathname), Overwrite);
    }
  }
}
