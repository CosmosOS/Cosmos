#pragma warning disable 162 
// The compiler doesn't like the fact that true == true might not necessarily be true.
using System;
using System.Collections.Generic;
using System.Text;
using Sys = Cosmos.System;

namespace TestKernel
{
    public class Kernel : Sys.Kernel
    {
        public Kernel() : base()
        {
            this.ClearScreen = false;
        }
        protected override void BeforeRun() { }

        public void DoRun()
        {
            this.Run();
        }

        protected override void Run()
        {
            IL2CPUTester.TestDriver.RunTests();

            //while (true) { } // Prevent the check from being run again, and display results.
        }
    }
}
#pragma warning restore 162