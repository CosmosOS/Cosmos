using System;
using Cosmos.Compiler.Builder;
using System.Drawing;
using Cosmos.Hardware;
using Cosmos.Kernel;

namespace SteveKernel
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

        public unsafe static void Init()
        {
            var xBoot = new Cosmos.Sys.Boot();
            xBoot.Execute();





            //ACPIManager.Init();

            //Console.ReadLine();


            //int addr = (int)VBE3.FindPM();
            int addr = (int)ACPIManager.RSDPAddress();

            Cosmos.Kernel.DebugMemoryViewer.View(addr);






        }

    }
}