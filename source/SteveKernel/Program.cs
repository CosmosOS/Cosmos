using System;
using Cosmos.Build.Windows;

namespace SteveKernel
{
    class Program
    {
        #region Cosmos Builder logic
        // Most users wont touch this. This will call the Cosmos Build tool
        [STAThread]
        static void Main(string[] args)
        {
            var xBuilder = new Builder();
            xBuilder.Build();
        }
        #endregion




        // Main entry point of the kernel
        public static void Init()
        {
            Console.WriteLine("Done booting");


            Cosmos.Hardware.PC.Bus.PCIBus.Init();
            Cosmos.Hardware.PC.Bus.PCIBus.DebugLSPCI();


            while (true)
                ;
        }
    }
}