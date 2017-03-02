using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using Sys = Cosmos.System;

namespace Cosmos.Kernel.Tests.DeviceInformation
{
    public class Kernel : Sys.Kernel
    {
        protected override void BeforeRun()
        {
            Console.WriteLine("Cosmos booted successfully. Type a line of text to get it echoed back.");
            Console.WriteLine(Cosmos.System.PCInfo.ProcessorInfo.ProcCpuinfo());
        }

        protected override void Run()
        {
            Cosmos.TestRunner.TestController.Completed();
        }
    }

}
