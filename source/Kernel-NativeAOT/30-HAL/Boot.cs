using System;

namespace Cosmos.HAL
{
    public class Boot
    {
        public static void Init()
        {
            Console.WriteLine("Cosmos.HAL booted successfully.");
            System.Boot.Init();
        }
    }
}
