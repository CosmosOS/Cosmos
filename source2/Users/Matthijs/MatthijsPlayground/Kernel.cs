using System;
using System.Collections.Generic;
using System.Text;
using Sys = Cosmos.System;
using Cosmos.Common.Extensions;
using System.IO;
using Cosmos.Debug.Kernel;

namespace MatthijsPlayground
{
    public class Kernel : Sys.Kernel
    {
        private class TestComparer : IEqualityComparer<int>
        {
            public bool Equals(int x, int y)
            {
                return x == y;
            }

            public int GetHashCode(int obj)
            {
                return obj.GetHashCode();
            }
        }
        protected override void BeforeRun()
        {
            Console.WriteLine("Cosmos booted successfully. Type a line of text to get it echoed back.");
        }

        private static byte[] mTestBytes;

        protected override void Run()
        {
            var xDict = new Dictionary<int, string>(new TestComparer());
            xDict.Add(1, "One");
            xDict.Add(2, "Two");
            DoSomething();
            Stop();
        }

        private static void DoSomething()
        {
            Console.WriteLine("Before Everything");

            //var xString = "";
            //var xDbg = new Debugger("kernel", "kernel");
            //xDbg.Send(xString);
            //var xSB = new StringBuilder();
            //xSB.Append("Hello");
            //xSB.Append("Hello");
            //var xDisplay = xSB.ToString();
            //Console.WriteLine(xDisplay.Length);
            //Console.WriteLine(xDisplay);
            
            Console.WriteLine("After everything");
            //WriteLine("Line2");
            //WriteLine("Line3");
        }

        private static void WriteLine(string line)
        {
            var xTheLine = line.ToUpper();
            Console.WriteLine(line);
        }
    }
}
