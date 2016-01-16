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
            var xList = new List<string> { "A", "B" };

            string[] xArray = new[] { "A", "B" };
            // var xItem = xList.First(); // mkrefany

            string xItem = string.Join("|", xArray);
            Console.WriteLine(xItem);
            mDebugger.Send($"xItem = {xItem}");
        }
    }
}

