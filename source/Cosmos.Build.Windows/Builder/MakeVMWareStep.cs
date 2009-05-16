using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;

namespace Cosmos.Compiler.Builder
{
    public class MakeVMWareStep : BuilderStep
    {
        public MakeVMWareStep(BuildOptions options)
            : base(options)
        {
        }



        override public void Execute()
        {
            Init();

            new MakeISOStep(options).Execute();  //TODO shouldnt builder make this ?
            string xPath = BuildPath + @"VMWare\";

            if (options.VMWareEdition == "VMWareServer") //HACK //BUG
            {
                xPath += @"Server\";
            }
            else
            {
                xPath += @"Workstation\";
            }

            buildFileUtils.RemoveReadOnlyAttribute(xPath + "Cosmos.nvram");
            buildFileUtils.RemoveReadOnlyAttribute(xPath + "Cosmos.vmsd");
            buildFileUtils.RemoveReadOnlyAttribute(xPath + "Cosmos.vmx");
            buildFileUtils.RemoveReadOnlyAttribute(xPath + "Cosmos.vmxf");
            buildFileUtils.RemoveReadOnlyAttribute(xPath + "hda.vmdk");

            Process.Start(xPath + @"Cosmos.vmx"); Finish();
        }





    }
}
