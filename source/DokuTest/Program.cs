using System;
using Cosmos.Build.Windows;

namespace DokuTest
{
    class Program
    {
        #region Cosmos Builder logic
        // Most users wont touch this. This will call the Cosmos Build tool
        [STAThread]
        static void Main(string[] args) {
            BuildUI.Run();
        }
        #endregion

        public static void Init()
        {
            Cosmos.Sys.Boot.Default();

            Console.WriteLine("*** COSMOS Operating System - Doku's Test Suite ***");
            Console.WriteLine("*** Ensoniq AudioPCI 1370 testing***");

            ES1370.test();

            Console.WriteLine("Shutting down computer");
            Cosmos.Sys.Deboot.ShutDown();            
        }
    }
}