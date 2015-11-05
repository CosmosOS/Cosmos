using System;
using System.Collections.Generic;
using System.Text;
using DuNodes.System;
using DuNodes.System.Base;
using DuNodes.System.Console.CommandManager;
using Sys = Cosmos.System;

namespace DuNodes
{
    public class Kernel : Sys.Kernel
    {
        protected override void BeforeRun()
        {
            Init.Initialisation();
        }

        protected override void Run()
        {
            Command cmd = new Command();

            while (true)
            {
                Console.Write("DNodes "+ ENV.currentPath +" # ");
                var input = Console.ReadLine();
                cmd.Handle(input);
                input = null;
            }
        }
    }
}