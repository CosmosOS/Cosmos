using System;

using Sys = Cosmos.System;

namespace Cosmos.Compiler.Tests.Exceptions
{
    using Cosmos.TestRunner;

    class DisposeTest : IDisposable
    {
        public void Dispose()
        {

        }
    }

    public class Kernel : Sys.Kernel
    {
        protected override void BeforeRun()
        {
            Console.WriteLine("Cosmos booted successfully, now start testing");
        }

        protected override void Run()
        {
            mDebugger.Send("Run");

            TestTryFinally.Execute();

            TestSimpleException();

            var xFilter = false;
            var xShouldCatch = false;
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

            xFilter = false;
            xShouldCatch = false;
            xCaught = false;
            xInFinally = false;
            //mDebugger.Send("START: Test throw Exception() in method and catch in caller without filter.");
            //try
            //{
            //    TestReturnSimpleException();
            //}
            //catch (Exception ex) when (xShouldCatch == true)
            //{
            //    Console.WriteLine("Caught filtered exception.");
            //    mDebugger.Send("EXCEPTION: " + ex.Message);
            //    xFilter = true;
            //}
            //catch (Exception ex)
            //{
            //    Console.WriteLine("Caught exception.");
            //    mDebugger.Send("EXCEPTION: " + ex.Message);
            //    xCaught = true;
            //}
            //finally
            //{
            //    Console.WriteLine("Finally");
            //    mDebugger.Send("EXCEPTION: Finally");
            //    xInFinally = true;
            //}
            //mDebugger.Send("END");
            //Assert.IsFalse(xFilter, "Should not reach filter block (4)");
            //Assert.IsTrue(xCaught, "Did not reach catch block (4)");
            //Assert.IsTrue(xInFinally, "Did not reach finally block (4)");

            xFilter = false;
            xShouldCatch = true;
            xCaught = false;
            xInFinally = false;
            //mDebugger.Send("START: Test throw Exception() in method and catch in caller with filter.");
            //try
            //{
            //    TestReturnSimpleException();
            //}
            //catch (Exception ex) when (xShouldCatch == true)
            //{
            //    Console.WriteLine("Caught filtered exception.");
            //    mDebugger.Send("EXCEPTION: " + ex.Message);
            //    xFilter = true;
            //}
            //finally
            //{
            //    Console.WriteLine("Finally");
            //    mDebugger.Send("EXCEPTION: Finally");
            //    xInFinally = true;
            //}
            //mDebugger.Send("END");
            //Assert.IsTrue(xFilter, "Did not reach filter block (5)");
            //Assert.IsTrue(xInFinally, "Did not reach finally block (5)");

            //TestThrowInUsing(); - the test will work when nested exceptions work

            TestController.Completed();
        }

        void TestThrowInUsing()
        {
            mDebugger.Send("Start: Test throwing exceptions in using blocks");
            try
            {
                using (var t = new DisposeTest())
                {
                    throw new Exception("Custom Exception in Using block");
                }
            }
            catch (Exception e)
            {
                mDebugger.Send("Exception: " + e.Message);
            }
            mDebugger.Send("End");
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
