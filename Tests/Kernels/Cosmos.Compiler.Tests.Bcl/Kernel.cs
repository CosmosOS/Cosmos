using System;
using System.Collections;

using Cosmos.TestRunner;
using Sys = Cosmos.System;

using Cosmos.Compiler.Tests.Bcl.CSharp;
using Cosmos.Compiler.Tests.Bcl.System;
using Cosmos.Compiler.Tests.Bcl.System.Collections.Generic;
using Cosmos.Compiler.Tests.Bcl.System.Text;
using Cosmos.Compiler.Tests.Bcl.System.Collections;

namespace Cosmos.Compiler.Tests.Bcl
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

                //// C#
                ObjectTest.Execute();
                WhileLoopTests.Execute();
                ForeachLoopTests.Execute();

                //mDebugger.Send("Thread test start of 500 ms");
                //ThreadTest.Execute();
                //mDebugger.Send("Thread test end");

                //DecimalTest.Execute();
                BitConverterTest.Execute();
                UnsafeCodeTest.Execute();
                EventsTest.Execute();
                RandomTests.Execute();
                ConvertTests.Execute();

                // System.Collections
                HashtableTest.Execute();

                // System.Collections.Generic
                ListTest.Execute();
                QueueTest.Execute();
                DictionaryTest.Execute();

                // System.Text
                StringBuilderTest.Execute();
                EncodingTest.Execute();
				
				GuidTest.Execute();

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
