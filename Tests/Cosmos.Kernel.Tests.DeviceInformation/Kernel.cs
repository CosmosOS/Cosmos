using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using Cosmos.System.PCInfo;
using Sys = Cosmos.System;

namespace Cosmos.Kernel.Tests.DeviceInformation
{
    public class Kernel : Sys.Kernel
    {
        protected override void BeforeRun()
        {
            Console.WriteLine("Cosmos booted successfully. Type a line of text to get it echoed back.");
            ProcessorInfo.WriteLine del = Console.Write;
            ProcessorInfo.ProcCpuinfo(del);
            Console.WriteLine("Out brand: " + Cosmos.System.PCInfo.ProcessorInfo.ListProcessors[0].Brand);
        }

        protected override void Run()
        {
            Console.ReadKey();
            Cosmos.TestRunner.TestController.Completed();
        }
    }

}
