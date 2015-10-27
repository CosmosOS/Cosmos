using System;
using System.Collections.Generic;
using System.Text;
using Sys = Cosmos.System;

namespace DuNodes
{
    public class Kernel : Sys.Kernel
    {
        protected override void BeforeRun()
        {
            Console.WriteLine("Cosmos booted successfully. Type a line of text to get it echoed back.");
        }

        protected override void Run()
        {
            Console.Write("Input: ");
            var input = Console.ReadLine();
            Console.Write("Text typed: ");
            Console.WriteLine(input);
        }
    }
}

//using System;
//using System.Collections.Generic;
//using System.Text;
//using DuNodes.Core.Base;
//using DuNodes_Core.Terminal.CommandManager;
//using Sys = Cosmos.System;

//namespace DuNodes
//{
//    public class Kernel : Sys.Kernel
//    {
//        protected override void BeforeRun()
//        {
//            Init.Initialisation(this);
//        }

//        protected override void AfterRun()
//        {
//            base.AfterRun();
//        }

//        protected override void Run()
//        {
//            Command cmd = new Command();
//            while (true)
//            {
//                Console.Write("DNodes # ");
//                var input = Console.ReadLine();
//                cmd.Handle(input);
//            }
//        }
//    }
//}

