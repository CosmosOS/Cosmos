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

    static public void Generate(string aBuildPath, string xInputPathname, string aIso) {
      string xPath = Path.Combine(aBuildPath, @"ISO\");
      RemoveFile(aIso);
      RemoveFile(Path.Combine(xPath, "output.bin"));

      File.Copy(xInputPathname, Path.Combine(xPath, "output.bin"));
      File.SetAttributes(Path.Combine(xPath, "isolinux.bin"), FileAttributes.Normal);

      var xOptions = new Options() {
        BootLoadSize = 4,
        // Why does this use CurrentDir? Is this some MS build thing? Why not above where it deletes it then?
        // TOOD: Investigate and fix this
        IsoFileName = Path.Combine(Environment.CurrentDirectory, aIso),
        BootFileName = Path.Combine(xPath, "isolinux.bin"),
        BootInfoTable = true
      };
      // TODO - Use move or see if we can do this without copying first the output.bin as they will start to get larger
      xOptions.IncludeFiles.Add(xPath);

      var xISO = new Iso9660Generator(xOptions);
      xISO.Generate();
    }
  }
}
