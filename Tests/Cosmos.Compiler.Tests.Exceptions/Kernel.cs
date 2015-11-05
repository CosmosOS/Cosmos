using System;
using System.Collections.Generic;
using System.Text;
using Sys = Cosmos.System;

namespace Cosmos.Compiler.Tests.Exceptions
{
    using Cosmos.TestRunner;

    public class Kernel : Sys.Kernel
    {
        private global::Cosmos.Debug.Kernel.Debugger mDebugger = new global::Cosmos.Debug.Kernel.Debugger("User", "Test");

        protected override void BeforeRun()
        {
            Console.WriteLine("Cosmos booted successfully, now start testing");
        }

        protected override void Run()
        {
            mDebugger.Send("Run");

            TestSimpleException();

            mDebugger.Send("START: Test throw Exception() in method and catch in caller.");
            try
            {
                TestReturnSimpleException();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Caught exception.");
                mDebugger.Send("EXCEPTION: " + ex.Message);
            }
            finally
            {
                Console.WriteLine("Finally");
                mDebugger.Send("EXCEPTION: Finally");
            }
            mDebugger.Send("END:");

            mDebugger.Send("START: Test throw nested Exception() in method and catch in caller.");
            try
            {
                TestThrowNestedException();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Caught exception.");
                mDebugger.Send("EXCEPTION: " + ex.Message);
            }
            finally
            {
                Console.WriteLine("Finally");
                mDebugger.Send("EXCEPTION: Finally");
            }
            mDebugger.Send("END:");

            TestController.Completed();
        }

        private void TestSimpleException()
        {
            mDebugger.Send("START: Test throw Exception() in method and catch in callee.");
            try
            {
                throw new Exception("throw new Exception()");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Caught exception.");
                mDebugger.Send("EXCEPTION: " + ex.Message);
            }
            mDebugger.Send("END:");
        }

        private void TestReturnSimpleException()
        {
            throw new Exception("throw new Exception()");
        }

        private void TestThrowNestedException()
        {
            try
            {
                try
                {
                    TestArgumentNullException(null);
                }
                catch (ArgumentNullException ex)
                {
                    Console.WriteLine("Caught nested exception.");
                    mDebugger.Send("EXCEPTION: " + ex.Message);
                }
                TestReturnSimpleException();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Caught exception.");
                mDebugger.Send("EXCEPTION: " + ex.Message);
            }
        }

        private void TestArgumentNullException(string arg)
        {
            if (arg == null)
            {
                throw new ArgumentNullException(arg, "arg can not be null.");
            }
        }
    }
}
