using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Diagnostics;
using Cosmos.Sys.Network;
using Cosmos.Hardware;
using Cosmos.Hardware.Network;
using Cosmos.Kernel;

namespace MatthijsTest
{
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

        public class TEst
        {
            public struct MyStruct
            {
                public int a;
                public int b;
            }

            public MyStruct mStruct;

            public void SetA(int a)
            {
                mStruct.a = a;
            }

            public void SetB(int b)
            {
                mStruct.b = b;
            }

            public int GetA()
            {
                return mStruct.a;
            }

            public int GetB()
            {
                return mStruct.b;
            }
        }

        public static unsafe void Init()
        {

            var xInit = true;
            if (xInit)
            {
                var xBoot = new Cosmos.Sys.Boot();
                xBoot.Execute(false);
            }

            var xObj = new TEst();
            xObj.SetA(5);
            xObj.SetB(33);
            Console.WriteLine("A: " + xObj.GetA());
            Console.WriteLine("B: " + xObj.GetB());

            int[] xTest = new int[] { 33, 5 };
            fixed (int* xTestAddr = &xTest[0])
            {
                Console.WriteLine("Int1: " + xTestAddr[0]);
                Console.WriteLine("Int2: " + xTestAddr[1]);
                var xStruct = *(TEst.MyStruct*)xTestAddr;
                var xStruct2 = xStruct;
                Console.WriteLine("StructInt1: " + xStruct.a);
                Console.WriteLine("StructInt2: " + xStruct.b);
                Console.WriteLine("StructInt3: " + xStruct2.a);
                Console.WriteLine("StructInt4: " + xStruct2.b);
                xObj.mStruct = *(TEst.MyStruct*)xTestAddr;
            }

            Console.WriteLine("A: " + xObj.GetA());
            Console.WriteLine("B: " + xObj.GetB());

            //int xCount = 0;
            //for (int i = 0; i < Device.Devices.Count; i++)
            //{
            //    if (Device.Devices[i] is BlockDevice)
            //    {
            //        xCount++;
            //    }
            //}

            //Console.WriteLine("Number of BlockDevices: " + xCount);

            //ulong a = 1;
            //ulong b = 2;
            //var c = a + b;
            //Interrupts.WriteNumber((uint)c, 32);
            //Console.WriteLine("");

            //MyATAController.Scan();
            //MyATADevice xDevice = null;
            //for (int i = 0; i < Device.Devices.Count; i++)
            //{
            //    if (Device.Devices[i] is MyATADevice)
            //    {
            //        xDevice = (MyATADevice)Device.Devices[i];
            //        break;
            //    }
            //}
            //if (xDevice != null)
            //{
            //    Console.WriteLine("Drive found!");
            //    Console.Write("Supports LBA28: ");
            //    if (xDevice.SupportsLBA28)
            //    {
            //        Console.WriteLine("Yes");
            //    }
            //    else
            //    {
            //        Console.WriteLine("No");
            //    }
            //    Console.Write("Supported UDMA: ");
            //    Interrupts.WriteNumber(xDevice.SupportedUDMA, 8);
            //    Console.WriteLine();
            //    Console.Write("Sector count: ");
            //    Interrupts.WriteNumber((uint)xDevice.BlockCount, 32);
            //    Console.WriteLine();

            //    var xDataToSend = new byte[512];
            //    for (int i = 0; i < 512; i++)
            //    {
            //        xDataToSend[i] = (byte)(i % 256);
            //    }
            //    xDevice.WriteBlock(2, xDataToSend);

            //}
            Console.WriteLine("Done");
            while (true)
                ;
        }
    }

    public static class DebugUtil
    {
        public static void Write(string data)
        {
            for (int i = 0; i < data.Length; i++)
            {
                Write(data[i]);
            }
        }

        private static void Write(char data)
        {
            Serial.WriteSerial(0, (byte)data);
        }

        public static void WriteUIntAsHex(uint data)
        {
            Write("0x");
            var xTemp = "0123456789ABCDEF";
            Write(xTemp[((int)((data >> 28) & 0xF))]);
            Write(xTemp[((int)((data >> 24) & 0xF))]);
            Write(xTemp[((int)((data >> 20) & 0xF))]);
            Write(xTemp[((int)((data >> 16) & 0xF))]);
            Write(xTemp[((int)((data >> 12) & 0xF))]);
            Write(xTemp[((int)((data >> 8) & 0xF))]);
            Write(xTemp[((int)((data >> 4) & 0xF))]);
            Write(xTemp[((int)((data) & 0xF))]);
        }

        public static void WriteLine(string data)
        {
            Write(data);
            Write("\r\n");
        }
    }
}