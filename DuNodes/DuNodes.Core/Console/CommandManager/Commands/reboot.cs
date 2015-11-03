using System;
using System.Collections;
using DuNodes.HAL;
using DuNodes.HAL.Extensions;
using DuNodes.System.Extensions;

namespace DuNodes.System.Console.CommandManager.Commands
{
    public class reboot : CommandBase
    {

        public override void launch(string[] args)
        {
            KernelExtensionsHAL.Reboot();
        }

        public override void cancelled()
        {
            throw new NotImplementedException();
        }

        public override void pause()
        {
            throw new NotImplementedException();
        }

        public override void finished()
        {
            throw new NotImplementedException();
        }
    }
}
