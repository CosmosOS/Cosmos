using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Mosa.Utility.IsoImage;

namespace Cosmos.Build.Common {
  public class IsoMaker {

    static protected void RemoveFile(string aPathname) {
      if (File.Exists(aPathname)) {
        File.Delete(aPathname);
      }
    }

    static public void Generate(string aBuildPath, string xInputPathname, string aIsoPathname) {
      string xPath = Path.Combine(aBuildPath, @"ISO");
      RemoveFile(aIsoPathname);
      
      // We copy and rename in the process to output.bin becaue the .cfg is currently
      // hardcoded to output.bin.
      string xOutputBin = Path.Combine(xPath, "output.bin");
      RemoveFile(xOutputBin);
      File.Copy(xInputPathname, xOutputBin);

      string xIsoLinux = Path.Combine(xPath, "isolinux.bin");
      File.SetAttributes(xIsoLinux, FileAttributes.Normal);

      var xOptions = new Options() {
        BootLoadSize = 4,
        IsoFileName = aIsoPathname,
        BootFileName = xIsoLinux,
        BootInfoTable = true
      };
      // TODO - Use move or see if we can do this without copying first the output.bin as they will start to get larger
      xOptions.IncludeFiles.Add(xPath);

      var xISO = new Iso9660Generator(xOptions);
      xISO.Generate();
    }
  }
}
