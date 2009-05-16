using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Cosmos.Compiler.Builder
{
    public class MakePXEStep : BuilderStep
    {
        public MakePXEStep(BuildOptions options)
            : base(options)
        {
        }



        override public void Execute()
        {
            Init();

            string xPath = BuildPath + @"PXE\";
            buildFileUtils.RemoveFile(xPath + @"Boot\output.bin");
            File.Move(BuildPath + "output.bin", xPath + @"Boot\output.bin");
            // *Must* set working dir so tftpd32 will set itself to proper dir
            Global.Call(xPath + "tftpd32.exe", "", xPath, false, true); 
            
            Finish();
        }





    }
}
