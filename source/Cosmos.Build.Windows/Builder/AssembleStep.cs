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
            Global.Call(ToolsPath + @"nasm\nasm.exe", String.Format("-g -f bin -o \"{0}\" \"{1}\"", BuildPath + "output.obj", AsmPath + "main.asm"), BuildPath);

            Finish();
        }





    }
}
