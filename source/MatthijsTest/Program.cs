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

        private static NetworkDevice mNet;

        public static unsafe void Init()
        {

            var xInit = true;
            if (xInit)
            {
                var xBoot = new Cosmos.Sys.Boot();
                xBoot.Execute(true);
            }

            //DebugUtil.Write("PCI Device count: ");
            //DebugUtil.WriteLine(PCIBus.Devices.Length.ToHex());
            //PCIDevice xDevice = null;

            //for (int i = 0; i < PCIBus.Devices.Length; i++)
            //{
            //     xDevice = PCIBus.Devices[i];
            //    DebugUtil.Write("  ");
            //    DebugUtil.WriteUIntAsHex((uint)i);
            //    DebugUtil.Write(": ");
            //    DebugUtil.WriteLine(xDevice.GetClassInfo());
            //    DebugUtil.Write("  Address count: ");
            //    var xAddresses = xDevice.NumberOfBaseAddresses();
            //    xDevice.EnableDevice();
            //    DebugUtil.WriteUIntAsHex((uint)xAddresses);
            //    DebugUtil.WriteLine("");
            //    for (int j = 0; j < xAddresses; j++)
            //    {
            //        DebugUtil.Write("    ");
            //        DebugUtil.WriteUIntAsHex((uint)j);
            //        DebugUtil.Write(": ");
            //        var xAddressSpace = xDevice.GetAddressSpace((byte)j);
            //        if (xAddressSpace == null)
            //        {
            //            DebugUtil.WriteLine(" **NULL**");
            //            continue;
            //        }
            //        DebugUtil.Write("Offset = ");
            //        DebugUtil.WriteUIntAsHex(xAddressSpace.Offset);
            //        DebugUtil.Write(", size = ");
            //        DebugUtil.WriteUIntAsHex(xAddressSpace.Size);
            //        DebugUtil.WriteLine("");
            //    }
            //}
            MyATAController.Scan();
            MyATADevice xDevice = null;
            for (int i = 0; i < Device.Devices.Count; i++)
            {
                if (Device.Devices[i] is MyATADevice)
                {
                    xDevice = (MyATADevice)Device.Devices[i];
                    break;
                }
            }
            if (xDevice != null)
            {
                Console.WriteLine("Drive found!");
                Console.Write("Supports LBA28: ");
                if (xDevice.SupportsLBA28)
                {
                    Console.WriteLine("Yes");
                }
                else
                {
                    Console.WriteLine("No");
                }
                Console.Write("Supported UDMA: ");
                Interrupts.WriteNumber(xDevice.SupportedUDMA, 8);
                Console.WriteLine();
                Console.Write("Sector count: ");
                Interrupts.WriteNumber((uint)xDevice.BlockCount, 32);
                Console.WriteLine();

                var xDataToSend = new byte[512];
                for(int i = 0; i < 512;i++){
                    xDataToSend[i]= (byte)(i % 256);
                }
                xDevice.WriteBlock(1, xDataToSend);

            }
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

        private static void Write(char data){
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