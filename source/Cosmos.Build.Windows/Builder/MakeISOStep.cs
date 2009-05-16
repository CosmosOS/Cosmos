using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Cosmos.Compiler.Builder
{
    public class MakeISOStep :BuilderStep 
    {
                public MakeISOStep(BuildOptions options)
            : base(options)
        {
        }

       


        override public void Execute()
        {
            Init(); 
          
            string xPath = BuildPath + @"ISO\";
            buildFileUtils.RemoveFile(BuildPath + "cosmos.iso");
            buildFileUtils.RemoveFile(xPath + "output.bin");
            buildFileUtils.CopyFile(BuildPath + "output.bin", xPath + "output.bin");
            // From TFS its read only, mkisofs doesnt like that
            buildFileUtils.RemoveReadOnlyAttribute(xPath + "isolinux.bin");
            Global.Call(ToolsPath + @"mkisofs.exe", @"-R -b isolinux.bin -no-emul-boot -boot-load-size 4 -boot-info-table -o ..\Cosmos.iso .", xPath);
            
            Finish(); 
        }



        

    }
}
