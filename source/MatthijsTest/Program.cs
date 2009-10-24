using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Reflection;
using System.Diagnostics;
using Cosmos.Sys.Network;
using Cosmos.Hardware;
using Cosmos.Hardware.Network;

namespace MatthijsTest
{
    public struct MyTest
    {
        public MyTest(int a, int b)
        {
            A = a;
            B = b;
            Sum = a + b;
        }

        public int A;
        public int B;
        public int Sum;
    }
	public class Program
	{
		#region Cosmos Builder logic
		// Most users wont touch this. This will call the Cosmos Build tool
		[STAThread]
		static void Main(string[] args)
		{
			Cosmos.Compiler.Builder.BuildUI.Run();
		}
		#endregion

        private static NetworkDevice mNet;

		public static void Init(){
            
            var xInit = true;
            if (xInit)
            {
                var xBoot = new Cosmos.Sys.Boot();
                xBoot.Execute(true);
            }

            for (int i = 0; i < Device.Devices.Count; i++)
            {
                if (Device.Devices[i].Type == Device.DeviceType.Network)
                {
                    mNet = (NetworkDevice)Device.Devices[i];
                    break;
                }
            }
            if (mNet == null)
            {
                Console.WriteLine("NIC not found!");
                while (true) ;
            }
            TCPIPStack.Init();
            
            TCPIPStack.ConfigIP(mNet, new IPv4Config(new IPv4Address(10, 0, 0, 2), new IPv4Address(255, 255, 255, 0)));
            var xDest = new IPv4Address(10, 0, 0, 1);
            TCPIPStack.Ping(xDest);
//                TCPIPStack.SendUDP(xDest, 632, 643, new byte[0]);
            //while (true) { TCPIPStack.Update(); }

            SendString("ABBA");
            Console.WriteLine("Klaar");
            
		}

        private static void SendString(string aStr)
        {
            Console.WriteLine("In SendString");
            var xData = new byte[aStr.Length];
            Console.WriteLine("Array created");
            for (int i = 0; i < aStr.Length; i++)
            {
                Console.WriteLine("In loop");
                xData[i] = (byte)aStr[i];
                Console.WriteLine("  After move");
            }
            Console.WriteLine("Create UDP package");
            var xUDP = new UDPPacket(xData);
            Console.WriteLine("Set DestAddr");
            xUDP.DestinationAddress = 0x0A000001;
            Console.WriteLine("Set Dest Port");
            xUDP.DestinationPort = 643;
            Console.WriteLine("GetData");
            var xTheData = xUDP.GetData();
            Console.WriteLine("Send");
            mNet.QueueBytes(xTheData);
        }

    }
}