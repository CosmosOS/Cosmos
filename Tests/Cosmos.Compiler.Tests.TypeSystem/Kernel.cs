using Cosmos.TestRunner;
using System;
using Sys = Cosmos.System;

namespace Cosmos.Compiler.Tests.TypeSystem
{
    public class Kernel : Sys.Kernel
    {
        protected override void BeforeRun()
        {
            Console.WriteLine("Cosmos booted successfully. Starting BCL tests now please wait...");
        }

        protected override void Run()
        {
            try
            {
                mDebugger.Send("Run");

                string xString = "a";
                Type xType = xString.GetType();
                if (xType == typeof(string))
                {
                    mDebugger.Send("Type is a string.");
                }

                TestController.Completed();
            }
            catch (Exception e)
            {
                mDebugger.Send("Exception occurred: " + e.Message);
                mDebugger.Send(e.Message);
                Console.WriteLine("Exception occurred");
                Console.WriteLine(e.Message);
                TestController.Failed();
            }
        }
    }
}
