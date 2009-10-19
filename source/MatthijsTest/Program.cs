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

		public static void Init(){
           
            var xInit = true;
            if (xInit)
            {
                var xBoot = new Cosmos.Sys.Boot();
                xBoot.Execute(false);
            }

            TCPIPStack.Init();
            NetworkDevice xNet = null;
            for (int i = 0; i < Device.Devices.Count; i++)
            {
                if (Device.Devices[i].Type == Device.DeviceType.Network)
                {
                    xNet = (NetworkDevice)Device.Devices[i];
                    break;
                }
            }
            if (xNet == null)
            {
                Console.WriteLine("NIC not found!");
                while (true) ;
            }
            TCPIPStack.ConfigIP(xNet, new IPv4Config(new IPv4Address(10, 0, 0, 1), new IPv4Address(255, 0, 0, 0)));
            var xDest = new IPv4Address(10, 255, 255, 255);
            TCPIPStack.SendUDP(xDest, 8365, 8654, new byte[] { 65, 65, 65, 65, 1, 2, 3, 4, 5, 66, 66, 66, 66 });
            Console.WriteLine("Klaar");
            while (true)
            {
                TCPIPStack.Update();
            }
            
		}

    }
}