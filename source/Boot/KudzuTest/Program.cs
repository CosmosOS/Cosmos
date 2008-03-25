using System;
using Cosmos.Build.Windows;
using RTLDriver = Cosmos.Hardware.Network.Devices.RTL8139;

namespace KudzuTest {
	class Program {
		#region Cosmos Builder logic
		// Most users wont touch this. This will call the Cosmos Build tool
		[STAThread]
		static void Main(string[] args) {
            new RTL8139().Test();
            BuildUI.Run();
        }
		#endregion

        // http://www.h7.dion.ne.jp/~qemu-win/HowToNetwork-en.html
        //  -Set to user and IP's
        // Arrays are not copied to TXBuffer, copying is also inefficient
        // Fixed - is only temproary, it only works because we dont have a GC yet
        // Ethernet frame checksum....
        // need to dig
        //   -Packet header in Frode code?
        //   -TSD not getting set right I think.. .need to look deeper

		public static void Init() {
            Cosmos.Kernel.Boot.Default();
			//System.Diagnostics.Debugger.Break();
            Console.WriteLine("Boot complete");

            //Cosmos.Hardware.PC.Bus.PCIBus.Init();
            //Console.ReadLine();
            Tests.DoAll();

            //Cosmos.Kernel.Temp.Kudzu.PCI.Test();

            // Load
            var xNICs = RTLDriver.RTL8139.FindRTL8139Devices();
            if (xNICs.Count == 0) {
                throw new Exception("Unable to find RTL8139 network card!");
            }
            var xNIC = xNICs[0];

            Console.WriteLine("Enabling network card!");
            Console.WriteLine(xNIC.Name);
            Console.WriteLine("Revision: " + xNIC.GetHardwareRevision());
            Console.WriteLine("MAC: " + xNIC.MACAddress);

            xNIC.Enable();
            xNIC.InitializeDriver();

            var xFrame = new Frame();
            //xFrame.Init1();
            xFrame.Init2();
            xNIC.TransmitRaw(xFrame.mData);

            Console.WriteLine("Done");
            while (true)
				;
		}

	}
}
