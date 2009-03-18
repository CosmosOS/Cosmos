using System;
using Cosmos.Compiler.Builder;
using HW = Cosmos.Hardware;
using Cosmos.Kernel;
using System.Collections.Generic;
using Cosmos.Sys.Network;

namespace Cosmos.Playground.SSchocke {
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
		public static void Init() {
            var xBoot = new Cosmos.Sys.Boot();
            xBoot.Execute();

            Console.WriteLine("Congratulations! You just booted SSchocke's C# code.");
            Console.WriteLine("Scanning for AMD PCNET Networks Cards...");

            HW.Network.NetworkDevice nic = null;
            foreach (HW.PCIDevice dev in HW.PCIBus.Devices)
            {
                if ((dev.VendorID == 0x1022) && (dev.DeviceID == 0x2000))
                {
                    nic = new HW.Network.Devices.AMDPCNetII.AMDPCNet(dev);
                    Console.WriteLine("Found AMD PCNet NIC on PCI " + dev.Bus + ":" + dev.Slot + ":" + dev.Function);
                    Console.WriteLine("NIC IRQ: " + dev.InterruptLine);
                    Console.WriteLine("NIC MAC Address: " + nic.MACAddress.ToString());
                    break;
                }
            }
            Console.WriteLine("Initializing NIC...");
            nic.Enable();

            Console.WriteLine("Initializing TCP Stack...");
            TCPIPStack.Init();
            TCPIPStack.ConfigIP(nic, new IPv4Config(new IPv4Address(192, 168, 20, 123), new IPv4Address(255, 255, 255, 0)));

            while (true)
            {
                TCPIPStack.Update();
            }

            Console.WriteLine("Press a key to shutdown...");
            Console.Read();
            Cosmos.Sys.Deboot.ShutDown();
		}

        /*private static void WriteBinaryBuffer(byte[] buffer)
        {
            foreach (byte b in buffer)
            {
                Console.Write(b.ToHex(2) + " ");
            }
            Console.WriteLine();
        }*/
	}
}