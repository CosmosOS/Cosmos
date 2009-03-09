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
                    nic = new AMDPCNet(dev);
                    Console.WriteLine("Found AMD PCNet NIC on PCI " + dev.Bus + ":" + dev.Slot + ":" + dev.Function);
                    Console.WriteLine("NIC IRQ: " + dev.InterruptLine);
                    Console.WriteLine("NIC MAC Address: " + nic.MACAddress.ToString());
                    break;
                }
            }
            //PCITest.Test();

            ARPPacket packet = new ARPPacket(nic.MACAddress.bytes);
            var xBytes = packet.GetData();
            Console.Write("Transmit Buffer: ");
            WriteBinaryBuffer(xBytes);

            Console.WriteLine("Initializing NIC...");
            nic.Enable();
            bool sent = false;
            while(true)
            {
                if ((nic.Ready == true) && (sent == false))
                {
                    Console.WriteLine("Sending Packet...");
                    nic.QueueBytes(xBytes, 0, xBytes.Length);
                    sent = true;
                }
                //Console.WriteLine("Status=" + nic.StatusRegister.ToHex(4));
                //Console.WriteLine("Status=" + nic.StatusRegister.ToHex(4) + ", BurstBus=" + nic.BurstBusControlRegister.ToHex(4));
                //Console.WriteLine("Mode=" + nic.ModeRegister.ToHex(4) + ", SWStyle=" + nic.SoftwareStyleRegister.ToHex(4));
                //Console.WriteLine("TXDesc0[0]=" + nic.GetTXDesc0()[0].ToHex(8) + ",TXDesc0[1]=" + nic.GetTXDesc0()[1].ToHex(8));
                //Console.WriteLine("TXDesc0[2]=" + nic.GetTXDesc0()[2].ToHex(8) + ",TXDesc0[3]=" + nic.GetTXDesc0()[3].ToHex(8));
                //Console.Read();
                while (nic.BytesAvailable() > 0)
                {
                    byte[] data = nic.ReceivePacket();
                    Console.WriteLine("Recv Data: Length=" + data.Length);
                    Console.Write("Data: ");
                    WriteBinaryBuffer(data);
                }
            }
            Console.WriteLine("Press a key to shutdown...");
            Console.Read();
            Cosmos.Sys.Deboot.ShutDown();
		}

        private static void WriteBinaryBuffer(byte[] buffer)
        {
            foreach (byte b in buffer)
            {
                Console.Write(b.ToHex(2) + " ");
            }
            Console.WriteLine();
        }
	}
}