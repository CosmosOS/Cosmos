using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.IO;

namespace Cosmos.Build.Common
{
    public class UsbMaker
    {
        static public void Generate(string aDrive, string aKernelFile)
        {
            string xDrive = aDrive + @":\";
            string xPathUSB = Path.Combine(CosmosPaths.Build, "USB");

            // Copy to USB device
            File.Copy(Path.Combine(xPathUSB, "mboot.c32"), xDrive + "mboot.c32", true);
            File.Copy(Path.Combine(xPathUSB, "syslinux.cfg"), xDrive + "syslinux.cfg", true);
            File.Copy(Path.Combine(xPathUSB, "ldlinux.c32"), xDrive + "ldlinux.c32");
            File.Copy(Path.Combine(xPathUSB, "libcom32.c32"), xDrive + "libcom32.c32");
            File.Copy(aKernelFile, xDrive + "Cosmos.bin", true);
            //File.Copy(Path.Combine(xPathUSB, "syslinux-x86.efi"), xDrive + "syslinux-x86.efi", true);
            //File.Copy(Path.Combine(xPathUSB, "syslinux-x64.efi"), xDrive + "syslinux-x64.efi", true);

            // Set MBR
            //
            // In future we might be able to bring this in house to reduce external calls.
            //   - syslinux-4.05\win\syslinux.c - has source we need.
            //   - http://www.fort-awesome.net/blog/2010/03/25/MBR_VBR_and_Raw_Disk
            //
            var xPSI = new ProcessStartInfo(Path.Combine(CosmosPaths.Tools, "syslinux.exe"), "-fma " + aDrive + ":");
            xPSI.UseShellExecute = false;
            xPSI.CreateNoWindow = true;
            Process.Start(xPSI);
        }
    }
}
