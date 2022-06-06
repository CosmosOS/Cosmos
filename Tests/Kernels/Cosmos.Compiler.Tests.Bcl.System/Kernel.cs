using System;
using System.Collections;
using Cosmos.Core.Memory;
using Cosmos.TestRunner;
using Sys = Cosmos.System;

namespace Cosmos.Compiler.Tests.Bcl.System
{
    public class Kernel : Sys.Kernel
    {
        protected override void BeforeRun()
        {
            Console.WriteLine("Cosmos booted successfully. Starting BCL System tests now please wait...");
        }

        protected override void Run()
        {
            try
            {
                mDebugger.Send("Run");


                // System
                ArrayTests.Execute();
                StringTest.Execute();
                ObjectTests.Execute();
                ByteTest.Execute();
                SByteTest.Execute();
                Int16Test.Execute();
                UInt16Test.Execute();
                Int32Test.Execute();
                UInt32Test.Execute();
                Int64Test.Execute();
                UInt64Test.Execute();
                CharTest.Execute();
                BooleanTest.Execute();
                SingleTest.Execute();
                DoubleTest.Execute();
                MathTest.Execute();
                ConvertTests.Execute();
                DateTimeTests.Execute();
                TimeSpanTests.Execute();

                int count = Heap.Collect();
                mDebugger.Send("Free");
                mDebugger.Send(count.ToString());

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
