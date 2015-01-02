using System;
using PlaygroundSystem;
using Sys = Cosmos.System;

namespace Playground
{
    public class Kernel : OurKernel
    {
        protected override void BeforeRun()
        {
            Console.WriteLine("Cosmos booted successfully. Type a line of text to get it echoed back.");
        }

        protected override void Run()
        {
            Console.WriteLine("Started");
            Console.ReadLine();
            Console.WriteLine("Done");

            base.DoIt();

        }
    }
}
