using System;

using Cosmos.System.PCInfo;
using Cosmos.TestRunner;
using Sys = Cosmos.System;

namespace Cosmos.Kernel.Tests.DeviceInformation
{
    public class Kernel : Sys.Kernel
    {
        protected override void BeforeRun()
        {
            Console.WriteLine("Cosmos booted successfully. Starting Device Information tests now please wait...");
        }

        protected override void Run()
        {
            ProcessorInfo.ProcCpuinfo(Console.WriteLine);
            ProcessorInfo.ProcCpuinfo(mDebugger.Send);

            Console.WriteLine("Out brand: " + ProcessorInfo.ListProcessors[0].Brand);

            TestController.Completed();
        }
    }
}
