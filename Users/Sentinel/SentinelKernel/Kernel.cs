using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using Sys = Cosmos.System;

namespace SentinelKernel
{
    public class Kernel : Sys.Kernel
    {
        protected override void BeforeRun()
        {
        }

        protected override void Run()
        {
            string a = "test";
            double b = 1.25;
            Console.WriteLine($"{a} = {b}");
            mDebugger.Send($"{a} = {b}");
        }
    }
}

