using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Cosmos.Compiler.Builder
{
    public class AssembleStep : BuilderStep
    {

        public AssembleStep (BuildOptions options)
            : base(options)
        {
        }


        override public void Execute()
        {
            Init();

            buildFileUtils.RemoveFile(BuildPath + "output.obj");
            var xFormat = "bin";
            if (IsELF)
            {
                xFormat = "elf";
            }
            Global.Call(ToolsPath + @"nasm\nasm.exe", String.Format("-g -f {0} -o \"{1}\" \"{2}\"", xFormat, BuildPath + "output.obj", AsmPath + "main.asm"), BuildPath);

            Finish();
        }


        public bool IsELF { get; set; }


    }
}
