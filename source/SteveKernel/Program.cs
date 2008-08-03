using System;
using Cosmos.Build.Windows;
using System.Drawing;

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

            Cosmos.Sys.Boot.Default();


            Cosmos.Hardware.VGAScreen.SetMode80x25xText();

            for (int i=0;i<1000; i++)
                Console.Write("sdkjfhdsf dfs sdf dsf sdf sdf sdf sf sdkjfhskjfhsdkjh");
        }
    }
}