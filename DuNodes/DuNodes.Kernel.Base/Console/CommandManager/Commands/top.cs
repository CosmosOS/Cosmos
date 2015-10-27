using Cosmos.System;

namespace DuNodes_Core.Terminal.CommandManager.Commands
{
    public class top : CommandBase
    {
        public top()
        {
            //Console.WriteLine("RAM Used | Free | Total : [" + Extensions.KernelExtensions.GetUsedMemory(Env.Kernel).ToString() + " | " + Extensions.KernelExtensions.GetFreeMemory(Env.Kernel).ToString() + " | " + Extensions.KernelExtensions.GetMemory(Env.Kernel).ToString() + "]");
            ////Environ.cpu
        }
    }
}
