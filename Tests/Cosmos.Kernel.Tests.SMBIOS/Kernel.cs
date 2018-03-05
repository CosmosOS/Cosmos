using System;
using System.Collections.Generic;
using System.Text;
using Sys = Cosmos.System;
using Cosmos.TestRunner;

namespace Cosmos.Kernel.Tests.SMBIOS
{
    public class Kernel : Sys.Kernel
    {
        protected override void BeforeRun()
        {
            try
            {
                Sys.PCInfo.ProcessorInfo.ProcCpuinfo(Console.WriteLine);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message); 
            }
            Console.WriteLine("Cosmos booted successfully. Type a line of text to get it echoed back.");
        }

        protected override void Run()
        {
            Console.WriteLine("Press enter to continue...");
            Console.ReadLine();
            TestController.Completed();
        }
    }
}
