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
            Console.WriteLine("Cosmos booted successfully.");
        }

        protected override void Run()
        {
            string[] a = { "a", "b" };
            string b = a[0];
        }
    }
}

