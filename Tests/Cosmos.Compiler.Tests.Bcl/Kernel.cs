using System;

using Cosmos.TestRunner;
using Sys = Cosmos.System;
using Cosmos.Compiler.Tests.Bcl.CSharp;
using Cosmos.Compiler.Tests.Bcl.System;
using Cosmos.Compiler.Tests.Bcl.System.Collections.Generic;

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

                // CSharp
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
                //DecimalTest.Execute();
                BitConverterTest.Execute();
                UnsafeCodeTest.Execute();
                DelegatesTest.Execute();

                // System.Collections.Generic
                ListTest.Execute();
                QueueTest.Execute();
                //DictionaryTest.Execute();

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
