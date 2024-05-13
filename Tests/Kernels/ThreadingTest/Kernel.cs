using System;
using Sys = Cosmos.System;
using Cosmos.System;
using Cosmos.TestRunner;

namespace ThreadingTests
{
    public class Kernel : Sys.Kernel
    {
        int variable = 0;
        int variableTwo = 0;
        int variableThree = 0;

        protected override void BeforeRun()
        {
            Global.Debugger.Send("Cosmos booted successfully. Let's Test Threading!");
        }

        protected override void Run()
        {
            try
            {
                Global.Debugger.Send("[MainThread] Inside Run method! Starting threads...");

                var threadOne = new Thread(ThreadOne);
                var threadTwo = new Thread(ThreadTwo);

                threadOne.Start();
                threadTwo.Start();

                variableThree = 3;

                //Since Run is not a thread call PIT wait
                System.Threading.Thread.Sleep(4000);

                //Cosmos.HAL.Global.PIT.Wait(3000);

                Global.Debugger.Send("[MainThread] Waited 4 sec.");

                Assert.AreEqual(variable, 1, "Changing global variable from thread works");

                Assert.AreEqual(variableTwo, 2, "Changing global variable from second thread works");

                Assert.AreEqual(variableThree, 3, "Changing global variable from main context works");

                TestController.Completed();
            }
            catch (Exception e)
            {
                mDebugger.Send("Exception occurred: " + e.Message);
                mDebugger.Send(e.Message);
                TestController.Failed();
            }
        }

        public void ThreadOne()
        {
            //Make thread WAITING
            Thread.Sleep(1000);

            Global.Debugger.Send("[FirstThread] Hello");

            variable = 1;

            Global.Debugger.Send("[FirstThread] Bye");
        }

        public void ThreadTwo()
        {
            //Make thread WAITING
            Thread.Sleep(2000);

            Global.Debugger.Send("[SecondThread] Hello");

            variableTwo = 2;

            Global.Debugger.Send("[SecondThread] Bye");
        }
    }
}
