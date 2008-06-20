using System;
using Cosmos.Build.Windows;
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
            Cosmos.Sys.Boot.Default();

            Commander shell = Commander.GetInstance();
            shell.Start();

            while (true)
                ;
        }
    }
}