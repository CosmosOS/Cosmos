using System;
using System.Collections;
using DuNodes.HAL;
using DuNodes.System.Extensions;

namespace DuNodes.System.Console.CommandManager.Commands
{
    public class reboot : CommandBase
    {

        public reboot()
        {
            KernelExtensionsHAL.Reboot();
            
        }
    }
}
