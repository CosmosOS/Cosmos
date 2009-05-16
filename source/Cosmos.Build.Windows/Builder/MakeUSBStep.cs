using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Cosmos.Compiler.Builder
{
    public class MakeUSBStep : BuilderStep
    {



        public MakeUSBStep(BuildOptions options)
            : base(options)
        {

        }



        override public void Execute()
        {
            Init();
            var aDrive = options.USBDevice;

            string xPath = BuildPath + @"USB\";
            buildFileUtils.RemoveFile(xPath + @"output.bin");
            File.Move(BuildPath + @"output.bin", xPath + @"output.bin");
            // Copy to USB device
            buildFileUtils.RemoveFile(aDrive + @":\output.bin");
            File.Copy(xPath + @"output.bin", aDrive + @":\output.bin");
            buildFileUtils.RemoveFile(aDrive + @":\mboot.c32");
            File.Copy(xPath + @"mboot.c32", aDrive + @":\mboot.c32");
            buildFileUtils.RemoveFile(aDrive + @":\syslinux.cfg");
            File.Copy(xPath + @"syslinux.cfg", aDrive + @":\syslinux.cfg");
            // Set MBR
            //TODO: Hangs on Windows 2008 - maybe needs admin permissions? Or maybe its not compat?
            // Runs from command line ok in Windows 2008.....

            Global.Call(ToolsPath + "syslinux.exe", "-fma " + aDrive + ":", ToolsPath, true, true);

            Finish();
        }





    }
}
