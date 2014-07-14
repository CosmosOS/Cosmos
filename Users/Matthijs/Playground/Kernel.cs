using System;
using System.Collections.Generic;
using System.Text;
using Cosmos.Hardware;
using Cosmos.Hardware.Drivers.PCI.Network;
using Cosmos.System.Network;
using Sys = Cosmos.System;

namespace Playground
{
    public class Kernel : Sys.Kernel
    {
        protected override void BeforeRun()
        {
            Console.WriteLine("Cosmos booted successfully. Type a line of text to get it echoed back.");
        }

        protected override void Run()
        {
            Console.WriteLine("Started");

            
            Console.WriteLine("Done");
            Console.ReadLine();
            Stop();
        }
    }
}