using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Cosmos.Compiler.Builder
{
    public class MakeVHDStep : BuilderStep
    {
                public MakeVHDStep(BuildOptions options)
            : base(options)
        {
        }



        override public void Execute()
        {
            Init();

            Environment.SetEnvironmentVariable("CosmosBuildPath", BuildPath);
            Global.Call(Environment.GetEnvironmentVariable("WinDir") + "\\system32\\diskpart /s " + BuildPath + "diskpart_script", "", BuildPath, true, false);
            Environment.SetEnvironmentVariable("CosmosBuildPath", "");
            
            Finish();
        }





    }
}
