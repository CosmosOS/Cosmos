using System;
using System.Collections.Generic;
using System.Text;
using Sys = Cosmos.System;
using Cosmos.Common.Extensions;

namespace MatthijsPlayground
{
    public class Kernel : Sys.Kernel
    {
        protected override void BeforeRun()
        {
            Console.WriteLine("Cosmos booted successfully. Type a line of text to get it echoed back.");
        }

        private static byte[] mTestBytes;

        protected override void Run()
        {
            DoSomething();
            Stop();
        }

        private static void DoSomething()
        {
            int xValue1 = 0x00000001;
            int xValue2 = 0x00000002;
            int xValue3 = 0x00000003;

            Console.WriteLine("Done");
            //WriteLine("Line1");
            //WriteLine("Line2");
            //WriteLine("Line3");
        }

        private static void WriteLine(string line)
        {
            Console.WriteLine(line);
        }
    }
}
