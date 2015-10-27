namespace DuNodes.System.Console.CommandManager.Commands
{
    public class top : CommandBase
    {
        public top()
        {
            Console.WriteLine("RAM Available : " + Extensions.KernelExtensionsHAL.GetMemory());
         //   Console.WriteLine("RAM Used | Free | Total : [" + Extensions.KernelExtensionsHAL.GetUsedMemory(Env.Kernel).ToString() + " | " + Extensions.KernelExtensionsSys.GetFreeMemory(Env.Kernel).ToString() + " | " + Extensions.KernelExtensionsSys.GetMemory(Env.Kernel).ToString() + "]");
            ////Environ.cpu
        }
    }
}
