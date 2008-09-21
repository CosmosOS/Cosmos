using System;
using Cosmos.Build.Windows;
using RTLDriver = Cosmos.Hardware.Network.Devices.RTL8139;

namespace Cosmos.Playground.Kudzu {
	class Program {
		[STAThread]
		static void Main(string[] args) {
            // This is here to run it on Windows and see results when necessary
            // Then can be run on Cosmos to see if values are the same
            //RTL8139.CreateTestFrame();
            BuildUI.Run();
        }

		public static void Init() {
            var xBoot = new Cosmos.Sys.Boot();
            xBoot.Execute();            
            Console.WriteLine("Boot complete");

            //PCITest.Test();
            //Tests.DoAll();
            //RTL8139.Test();
            //Debugger.Main();

            Console.WriteLine("Done - Waiting");
            Console.ReadLine();
            //TODO: Make this automatically called after Init if no other shut downs are called
            Cosmos.Sys.Deboot.Halt();
		}

        public static void DoLoop() {
            int i = 0;
            while (i < int.MaxValue) {
                i++;
                Console.WriteLine(i);
            }
        }
    
    }
}
