using System;
using System.Threading;
using Cosmos.Build.Windows;
using Cosmos.Sys;

namespace EsxTest
{
    class Program
    {
        #region Cosmos Builder logic
        // Most users wont touch this. This will call the Cosmos Build tool
        [STAThread]
        static void Main(string[] args)
        {
            BuildUI.Run();
        }

        #endregion

        // Main entry point of the kernel
        public static void Init()
        {
            Cosmos.Sys.Boot.Default();
            Console.WriteLine("Sleep 3000 ESX!");
            Thread.Sleep(3000);
            Console.WriteLine("Reboot");
            Deboot.Reboot();            
        }

    }
}
