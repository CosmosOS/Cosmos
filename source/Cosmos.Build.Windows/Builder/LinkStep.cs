using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Cosmos.Compiler.Builder
{
    public class LinkStep : BuilderStep
    {
        public LinkStep(BuildOptions options)
            : base(options)
        {
        }




        override public void Execute()
        {
            Init();

            buildFileUtils.RemoveFile(BuildPath + "output.bin");
            if (IsELF)
            {
                Global.Call(ToolsPath + @"cygwin\ld.exe", String.Format("-Ttext 0x500000 -Tdata 0x200000 -e Kernel_Start -o \"{0}\" \"{1}\"", "output.bin", "output.obj"), BuildPath);
            }
            else
            {
                File.Move(Path.Combine(BuildPath, "output.obj"), Path.Combine(BuildPath, "output.bin"));
            }
            buildFileUtils.RemoveFile(BuildPath + "output.obj");

            Finish();
        }

        public bool IsELF
        {
            get;
            set;
        }





    }
}
