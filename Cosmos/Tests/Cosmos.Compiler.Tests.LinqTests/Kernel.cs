using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Cosmos.TestRunner;

using Sys = Cosmos.System;

namespace Cosmos.Compiler.Tests.LinqTests
{
    public class Kernel : Sys.Kernel
    {
        protected override void BeforeRun()
        {
            Console.WriteLine("Cosmos booted successfully.");
        }

        protected override void Run()
        {
            try
            {
                var xList = new List<int> { 1, 2, 3, 4, 5 };
                foreach (int s in xList)
                {
                    mDebugger.Send("s == " + s);
                    Console.WriteLine(s);
                }

                //foreach (int s in xList.Where(x => x == 3))
                //{
                //    mDebugger.Send("s == " + s);
                //    Console.WriteLine(s);
                //}
                TestController.Completed();
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception occurred");
                Console.WriteLine(e.Message);
                mDebugger.Send("Exception occurred: " + e.Message);
                TestController.Failed();
            }
        }
    }
}
