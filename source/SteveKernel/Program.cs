using System;
using Cosmos.Build.Windows;
using Cosmos.Hardware.PC.Bus;
using Cosmos.FileSystem;
using Cosmos.Hardware;
using System.Diagnostics;


namespace SteveKernel
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
        
        // Main entry point of the kernel
        public static void Init()
        {
            VGA.Test();
        }
    }
}