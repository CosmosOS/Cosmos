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

            var xCaught = false;
            var xInFinally = false;
            mDebugger.Send("START: Test throw Exception() in method and catch in caller.");
            try
            {
                TestReturnSimpleException();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Caught exception.");
                mDebugger.Send("EXCEPTION: " + ex.Message);
                xCaught = true;
            }
            finally
            {
                Console.WriteLine("Finally");
                mDebugger.Send("EXCEPTION: Finally");
                xInFinally = true;
            }
            mDebugger.Send("END");
            Assert.IsTrue(xCaught, "Did not reach catch block (1)");
            //Assert.IsTrue(xInFinally, "Did not reach finally block (1)");
            xCaught = false;
            xInFinally = false;

            mDebugger.Send("START: Test throw nested Exception() in method and catch in caller.");
            try
            {
                TestThrowNestedException();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Caught exception.");
                mDebugger.Send("EXCEPTION: " + ex.Message);
                xCaught = true;
            }
            finally
            {
                Console.WriteLine("Finally");
                mDebugger.Send("EXCEPTION: Finally");
                xInFinally = true;
            }
            mDebugger.Send("END:");

            //Assert.IsTrue(xCaught, "Did not reach catch block (2)");
            //Assert.IsTrue(xInFinally, "Did not reach finally block (2)");
            xCaught = false;
            xInFinally = false;

            mDebugger.Send("START: Test throw exception in method, while using it as a second parameter for another call");
            try
            {
                ThrowExceptionInMethodWithReturnValue();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Caught exception.");
                mDebugger.Send("EXCEPTION: " + ex.Message);
                xCaught = true;
            }
            finally
            {
                Console.WriteLine("Finally");
                mDebugger.Send("EXCEPTION: Finally");
                xInFinally = true;
            }
            mDebugger.Send("END:");

            Assert.IsTrue(xCaught, "Did not reach catch block (3)");
            //Assert.IsTrue(xInFinally, "Did not reach finally block (3)");

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

        private void ThrowExceptionInMethodWithReturnValue()
        {
            Console.WriteLine("A" + GetValueError());
        }

        private int GetValueError()
        {
            throw new Exception("Error occurred");
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
