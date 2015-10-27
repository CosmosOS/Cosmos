using System;
using System.Collections.Generic;
using System.Text;
using DuNodes.System.Base;
using DuNodes.System.Console.CommandManager;
using Sys = Cosmos.System;

namespace DuNodes
{
    public class Kernel : Sys.Kernel
    {
        protected override void BeforeRun()
        {
            Init.Initialisation(this);
        }

        protected override void Run()
        {
            Command cmd = new Command();
            while (true)
            {
                Console.Write("DNodes # ");
                var input = Console.ReadLine();
                cmd.Handle(input);
            }
        }
    }
}