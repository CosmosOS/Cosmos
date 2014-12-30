using System;
using PlaygroundSystem;
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

            SystemGlobal.Execute();

            Console.WriteLine("Done");
            Console.ReadLine();
            Stop();
        }
    }
}
