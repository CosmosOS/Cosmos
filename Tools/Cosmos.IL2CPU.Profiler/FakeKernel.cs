using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.IL2CPU.Profiler
{
    public class FakeKernel: Cosmos.System.Kernel
    {
        protected override void BeforeRun()
        {
            Console.WriteLine("Cosmos booted successfully. Type a line of text to get it echoed back.");
        }

        protected override void Run()
        {
            Console.Write("Enter some text to be echoed back to you: ");
            Console.WriteLine(Console.ReadLine());
        }
    }
}