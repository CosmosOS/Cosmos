using System;
using Cosmos.Build.Windows;
using RTLDriver = Cosmos.Hardware.Network.Devices.RTL8139;

namespace KudzuTest {
	class Program {
		[STAThread]
		static void Main(string[] args) {
            //RTL8139.Test();
            BuildUI.Run();
        }

		public static void Init() {
            Cosmos.Sys.Boot.Default();
			//System.Diagnostics.Debugger.Break();
            Console.WriteLine("Boot complete");

            //PCITest.Test();
            //Tests.DoAll();
            RTL8139.Test();

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
