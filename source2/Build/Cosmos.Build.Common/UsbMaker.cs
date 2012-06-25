using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.IO;

namespace Cosmos.Build.Common {
  public class UsbMaker {

    static protected void RemoveFile(string aPathname) {
      if (File.Exists(aPathname)) {
        File.Delete(aPathname);
      }
    }

    static public void Generate(string aDrive) {
      string xDrive = aDrive + @":\";

      //string xPath = BuildPath + @"C:\Users\Atmoic\AppData\Roaming\Cosmos User Kit\Build\USB";
      string xPath =  @"C:\Users\Atmoic\AppData\Roaming\Cosmos User Kit\Build\USB";

      // Copy to USB device
      File.Copy(@"d:\source\Cosmos\source2\Demos\Guess\bin\Debug\Guess.obj", xDrive + "Cosmos.bin", true);
      File.Copy(Path.Combine(xPath, "mboot.c32"), xDrive + "mboot.c32", true);
      File.Copy(Path.Combine(xPath, "syslinux.cfg"), xDrive + "syslinux.cfg", true);

      // Set MBR
      //
      // In future we might be able to bring this in house, but will be some work and prboably not worth it.
      // Syslinux modifies MBR but also writes out a hidden ldlinux.sys file.
      // Not sure how it points to the ldlinux file. Prob best to ask on syslinux list if we decide to go
      // this route.
      // syslinux-4.05\win\syslinux.c - has source we need...
      // http://www.fort-awesome.net/blog/2010/03/25/MBR_VBR_and_Raw_Disk
      //
      string xToolsPath = @"C:\Users\Atmoic\AppData\Roaming\Cosmos User Kit\Build\Tools";
      var xPSI = new ProcessStartInfo(Path.Combine(xToolsPath, "syslinux.exe"), "-fma " + aDrive + ":");
      xPSI.CreateNoWindow = true;
      Process.Start(xPSI);
    }
  }
}
