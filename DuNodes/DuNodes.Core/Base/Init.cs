using System;

namespace DuNodes.Core.Base
{
    public static class Init
    {
        public static void Initialisation(Cosmos.System.Kernel kernel)
        {
            Console.WriteLine(".Booting DuNodes Alpha 0.1 R01", ConsoleColor.Blue, true);
            Console.WriteLine("..Checking prerequisites", ConsoleColor.Blue, true);
            Console.WriteLine("...RAM : " + DuNodes.Kernel.Base.Extensions.KernelExtensions.GetMemory(kernel) + "", ConsoleColor.Blue, true);
            if (DuNodes.Kernel.Base.Extensions.KernelExtensions.GetMemory(kernel) < 20)
            {
                Console.WriteLine("Minimum 20MB RAM TO LAUNCH", ConsoleColor.Red);
                DuNodes.Kernel.Base.Extensions.KernelExtensions.SleepSeconds(kernel, 5);
                DuNodes.Kernel.Base.Extensions.KernelExtensions.Shutdown(kernel);
            }
            Console.WriteLine("....Creating Env", ConsoleColor.Blue, true);
            //DuNodes.Kernel.Base.Env.Kernel = kernel;
            //Load everything we want before creating memmanger
            Console.WriteLine("....", ConsoleColor.Blue, true);
            //MemoryManager.Init();

        }
    }
}
