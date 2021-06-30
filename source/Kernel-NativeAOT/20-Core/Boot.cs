using System;
using System.Runtime.InteropServices;

namespace Cosmos.Core
{
    public class Boot
    {
        [UnmanagedCallersOnly(EntryPoint = "kernel_main", CallingConvention = CallingConvention.StdCall)]
        public static void Init()
        {
            Console.WriteLine("Cosmos.Core booted successfully.");
            HAL.Boot.Init();
        }
    }
}
