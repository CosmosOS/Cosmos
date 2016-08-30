using Cosmos.TestRunner;
using System;
using System.Collections.Generic;
using System.Text;
using Sys = Cosmos.System;

namespace Cosmos.Compiler.Tests.ConsoleDrawTest
{
    public class Kernel : Sys.Kernel
    {
        protected override void BeforeRun()
        {
            new ConsoleDraw.Windows.Alert("ConsoleDraw has successfully been compiled for the C# Open Source Managed Operating System.", new ConsoleDraw.Windows.Base.FullWindow(0, 0, Console.WindowWidth, Console.WindowHeight, null));
            TestController.Completed();
        }

        protected override void Run()
        {
        }
    }
}
