using System;
using System.IO;
using Cosmos.Compiler.Builder;
using Cosmos.Hardware;
using Cosmos.Sys;

namespace Mouse_Test
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
            Console.WriteLine("Loading Mouse");
            Mouse.Initialize();
            Console.WriteLine("Mouse Loaded");
            //VGAScreen.SetMode640x480x2();
            bool x = true;
            while (x == true)
            {
                Console.WriteLine("Mouse X:" + Mouse.X);
                Console.WriteLine("Mouse Y:" + Mouse.Y);
                PIT.Wait(100);
                Console.Clear();
            }
            Cosmos.Sys.Deboot.ShutDown();
        }
    }
}
