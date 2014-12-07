using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;

namespace Cosmos.Build.Common {
  public class IsoMaker {

    static public void Generate(string aBuildPath, string xInputPathname, string aIsoPathname) {
      string xPath = Path.Combine(aBuildPath, @"ISO");
      if (File.Exists(aIsoPathname)) {
        File.Delete(aIsoPathname);
      }
      
      string xIsoLinux = Path.Combine(xPath, "isolinux.bin");
      File.SetAttributes(xIsoLinux, FileAttributes.Normal);

       var xPSI = new ProcessStartInfo(
           Path.Combine(CosmosPaths.Tools, "mkisofs.exe"),
           String.Format("-R -b \"{0}\" -no-emul-boot -boot-load-size 4 -boot-info-table -o \"{1}\" \"{2}\"",
               xIsoLinux,               aIsoPathname, xPath)
       );
       xPSI.UseShellExecute = false;
       xPSI.CreateNoWindow = true;
       Process.Start(xPSI);
    }
  }
}
