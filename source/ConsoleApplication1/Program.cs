using System;
using Cosmos.Compiler.Builder;

namespace ConsoleApplication1
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

            float x;
            x = 10.5F;
            x = x / 2;

            Cosmos.Sys.Deboot.ShutDown();
        }
    }
}
