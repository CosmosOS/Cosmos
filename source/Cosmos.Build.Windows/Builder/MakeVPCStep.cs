using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;

namespace Cosmos.Compiler.Builder
{
    public class MakeVPCStep : BuilderStep
    {

        public MakeVPCStep(BuildOptions options)
            : base(options)
        {
        }


        override public void Execute()
        {
            Init();

            new MakeISOStep(options).Execute();  //TODO shouldnt builder make this ?
            string xPath = BuildPath + @"VPC\";
            buildFileUtils.RemoveReadOnlyAttribute(xPath + "Cosmos.vmc");
            buildFileUtils.RemoveReadOnlyAttribute(xPath + "hda.vhd");
            Process.Start(xPath + "Cosmos.vmc");
            Finish();
        }





    }
}
