using System;
using System.IO;
using Cosmos.Compiler.Builder;
using Cosmos.Hardware;
using Cosmos.Sys;

namespace VGA_with_keyboard_input
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
            var xBoot = new Cosmos.Sys.Boot();
            xBoot.Execute();
            Console.Clear();
            Console.WriteLine("Loading PIT...");
            Console.WriteLine("Waiting 3 secs...");
            PIT.Wait(3000);
            Console.WriteLine();
            bool x = true;
            while (x == true)
            {
                Console.Read();
                x = false;
            }
            Cosmos.Sys.Deboot.ShutDown();
        }
    }
}
