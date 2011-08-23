using System;
using System.Collections.Generic;
using System.Text;
using Sys = Cosmos.System;
using Orvid.Graphics;

namespace CrashTest
{
    public class Kernel : Sys.Kernel
    {
        public Kernel()
        {
            base.ClearScreen = true;
        }

        protected override void BeforeRun()
        {
            Console.WriteLine("Hello, and welcome to the TestOS by Orvid.");
            Console.WriteLine("To see a list of supported commands, type '?'");
        }

        protected override void Run()
        {
            Monitor m = new Monitor();
            Console.WriteLine("Just one more test.");
            while (true)
            {

            }
        }
    }
}
