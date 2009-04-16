using System;
using Cosmos.Compiler.Builder;

namespace Cosmos.Playground.Xenni
{
	class Program
    {
		[STAThread]
		static void Main(string[] args)
        {
            BuildUI.Run();
        }

		// Main entry point of the kernel
		public static void Init()
        {
            var xBoot = new Cosmos.Sys.Boot();
            xBoot.Execute();
            Console.WriteLine("");
            Console.WriteLine("Test of MultiArrays");
            Console.Read();

            MULTIARRAYTEST();
		}

        public static void MULTIARRAYTEST()
        {
            String[,] MultiArray = new String[3, 3];
            MultiArray[0, 0] = "0,0";
            MultiArray[0, 1] = "0,1";
            MultiArray[0, 2] = "0,2";
            MultiArray[1, 0] = "1,0";
            MultiArray[1, 1] = "1,1";
            MultiArray[1, 2] = "1,2";
            MultiArray[2, 0] = "2,0";
            MultiArray[2, 1] = "2,1";
            MultiArray[2, 2] = "2,2";
        }
	}
}