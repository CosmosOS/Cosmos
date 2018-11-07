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

                // C#
                WhileLoopTests.Execute();
                ForeachLoopTests.Execute();

                // System
                ObjectTests.Execute();
                ArrayTests.Execute();
                StringTest.Execute();
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

                //mDebugger.Send("Thread test start of 500 ms");
                //ThreadTest.Execute();
                //mDebugger.Send("Thread test end");

                //DecimalTest.Execute();
                BitConverterTest.Execute();
                UnsafeCodeTest.Execute();
                DelegatesTest.Execute();
                RandomTests.Execute();

                // System.Collections
                HashtableTest.Execute();

                // System.Collections.Generic
                ListTest.Execute();
                QueueTest.Execute();       
                DictionaryTest.Execute();

                // System.Text
                StringBuilderTest.Execute();
                EncodingTest.Execute();

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
