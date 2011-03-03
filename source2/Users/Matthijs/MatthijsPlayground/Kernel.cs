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
            var xBytes = new byte[] { 65, 66, 65, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            var xStr = ByteConverter.GetAsciiString(xBytes, 0, 6);
            

            Console.WriteLine("Done");
            Stop();
        }

        private static void DoSomething()
        {
            WriteLine("Line1");
            WriteLine("Line2");
            WriteLine("Line3");
        }

        private static void WriteLine(string line)
        {
            Console.WriteLine(line);
        }
    }
}
