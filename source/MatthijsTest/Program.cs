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
using System.Drawing;
using Cosmos.Kernel;

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

            Heap.EnableDebug = false;

            VGAScreen.SetMode320x200x8();

            VGAScreen.SetPaletteEntry(0, 0x00, 0x00, 0x00); //Black  (Background)
            VGAScreen.SetPaletteEntry(1, 0xFF, 0xFF, 0xFF); //White  (Walls)
            VGAScreen.SetPaletteEntry(2, 0xFF, 0xBB, 0xBB); //Peach  (Dead Snake)
            VGAScreen.SetPaletteEntry(3, 0x00, 0xFF, 0x00); //Green  (Player 1's Snake)
            VGAScreen.SetPaletteEntry(4, 0x00, 0x00, 0xFF); //Blue   (Player 2's Snake)
            VGAScreen.SetPaletteEntry(5, 0xFF, 0x00, 0x00); //Red    (Player 3's Snake)
            VGAScreen.SetPaletteEntry(6, 0xFF, 0xFF, 0x00); //Yellow (Player 4's Snake)

//            VGAScreen.Clear(1);

            uint xColor = 0;
            for (uint x = 0; x < 320; x++)
            {
                if ((x % 4) == 0)
                {
                    if (xColor == 0)
                    {
                        xColor = 2;
                    }
                    else
                    {
                        xColor = 0;
                    }
                }
                for (uint y = 0; y < 200; y++)
                {
                    VGAScreen.SetPixel320x200x8(x, y, xColor);
                }
            }
            while (true) ;

            

            //SendString("ABBA");
            
            
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