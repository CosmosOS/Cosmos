using System;
using Cosmos.Build.Windows;
using RTLDriver = Cosmos.Hardware.Network.Devices.RTL8139;

namespace KudzuTest {
	class Program {
		[STAThread]
		static void Main(string[] args) {
            RTL8139.Test();
            BuildUI.Run();
        }

        // http://www.h7.dion.ne.jp/~qemu-win/HowToNetwork-en.html
        //  -Set to user and IP's
        // Arrays are not copied to TXBuffer, copying is also inefficient
        // Fixed - is only temproary, it only works because we dont have a GC yet
        // Ethernet frame checksum....
        // need to dig
        //   -Packet header in Frode code?
        //   -TSD not getting set right I think.. .need to look deeper

        public static void DoLoop() {
            int i = 0;
            while (i < int.MaxValue) {
                i++;
                Console.WriteLine(i);
            }
        }

		public static void Init() {
            Cosmos.Sys.Boot.Default();
			//System.Diagnostics.Debugger.Break();
            Console.Clear();
            Console.WriteLine("Boot complete");

            //PCITest.Test();
            //Tests.DoAll();
            RTL8139.Test();

            //TODO: Make this automatically called after Init if no other shut downs are called
            Cosmos.Sys.Deboot.Halt();
		}

	}
}
