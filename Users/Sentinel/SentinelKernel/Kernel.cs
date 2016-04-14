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
            byte[] a = { 0x01, 0x02 };
            char[] b = { 'a', 'b' };
            string[] c = { "a", "b" };
            byte a1 = a[0];
            char b1 = b[0];
            string c1 = c[0];
        }
    }
}