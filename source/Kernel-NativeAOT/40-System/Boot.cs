using System;

namespace Cosmos.System
{
    public class Boot
    {
        public static void Init()
        {
            Console.WriteLine("Cosmos.System booted successfully.");

            var kernel = new Kernel();
            kernel.BeforeRun();

            while (true)
            {
                kernel.Run();
            }
        }
    }
}
