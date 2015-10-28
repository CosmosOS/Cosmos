using System;
using System.Collections;
using DuNodes.HAL;
using DuNodes.System.Extensions;

namespace DuNodes.System.Console.CommandManager.Commands
{
    public class shutdown : CommandBase
    {

        public shutdown()
        {
            KernelExtensionsHAL.Halt();
            Console.WriteLine("Not Implemented Yet", ConsoleColor.DarkRed);
        }
    }
}
