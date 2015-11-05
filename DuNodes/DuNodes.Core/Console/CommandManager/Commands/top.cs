using System;
using DuNodes.HAL.Extensions;

namespace DuNodes.System.Console.CommandManager.Commands
{
    public class top : CommandBase
    {

        public override void launch(string[] args)
        {
            Console.WriteLine("RAM Available | Used by kernel | ??? : " + KernelExtensionsHAL.GetMemory() + " | " + KernelExtensionsHAL.GetUsedMemoryByKernel() + " | " + KernelExtensionsHAL.GetUsedMemoryTotal());
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
