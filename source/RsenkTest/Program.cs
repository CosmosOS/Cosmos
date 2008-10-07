using System;
using Cosmos.Compiler.Builder;
using RsenkTest;

namespace CosmosBoot
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

            Commander shell = Commander.GetInstance();
            shell.Start();

            while (true) ;
        }
    }
}