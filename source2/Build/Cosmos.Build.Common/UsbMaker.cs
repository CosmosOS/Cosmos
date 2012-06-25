using System;
using System.Collections.Generic;
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

    static public void Generate() {
      string aDrive = "G";

      //string xPath = BuildPath + @"C:\Users\Atmoic\AppData\Roaming\Cosmos User Kit\Build\USB";
      string xPath =  @"C:\Users\Atmoic\AppData\Roaming\Cosmos User Kit\Build\USB";

      // Why do we copy it twice???
      RemoveFile(Path.Combine(xPath, "Cosmos.bin"));
      File.Copy(@"d:\source\Cosmos\source2\Demos\Guess\bin\Debug\Guess.obj", Path.Combine(xPath, "Cosmos.bin"));

      // Copy to USB device
      RemoveFile(aDrive + @":\Cosmos.bin");
      File.Copy(Path.Combine(xPath, "Cosmos.bin"), aDrive + @":\Cosmos.bin");

      RemoveFile(aDrive + @":\mboot.c32");
      File.Copy(Path.Combine(xPath, "mboot.c32"), aDrive + @":\mboot.c32");
      
      RemoveFile(aDrive + @":\syslinux.cfg");
      File.Copy(Path.Combine(xPath, "syslinux.cfg"), aDrive + @":\syslinux.cfg");

      // Set MBR
      //TODO: Hangs on Windows 2008 - maybe needs admin permissions? Or maybe its not compat?
      // Runs from command line ok in Windows 2008.....
      //Global.Call(ToolsPath + "syslinux.exe", "-fma " + aDrive + ":", ToolsPath, true, true);

      // http://www.fort-awesome.net/blog/2010/03/25/MBR_VBR_and_Raw_Disk
    }
  }
}
