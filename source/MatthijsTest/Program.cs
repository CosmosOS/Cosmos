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

		public static unsafe void Init(){

            var xInit = false;
            if (xInit)
            {
                var xBoot = new Cosmos.Sys.Boot();
                xBoot.Execute(true);
            }

            Cosmos.Debug.Debugger.Send("Hello, World!");
            while (true)
                ;

            //for (int i = 0; i < Device.Devices.Count; i++)
            //{
            //    if (Device.Devices[i].Type == Device.DeviceType.Network)
            //    {
            //        mNet = (NetworkDevice)Device.Devices[i];
            //        break;
            //    }
            //}
            //if (mNet != null)
            //{
            //    mNet.Enable();
            //    var xPkt = new UDPPacket(0x0A000002, 15, 0x0A000001, 16, new byte[] { 65, 66, 67, 68, 69 });
            //    var xEPkt = new EthernetPacket(xPkt.GetData());
            //    mNet.QueueBytes(xEPkt.GetData());
            //}
            //Console.WriteLine("Done!");
            //while (true)
            //{
            //    if (mNet != null)
            //    {
            //        TCPIPStack.Update();
            //    }
            //}

            //Heap.EnableDebug = false;
            //DebugUtil.SendNumber("Program", "DeviceCount", (uint)Device.Devices.Count, 32);
            //PCIDevice xVGADev = null;
            //for (int i = 0; i < PCIBus.Devices.Length; i++)
            //{
            //    var xPCIDev = PCIBus.Devices[i];
            //    if (xPCIDev.ClassCode == 3 && xPCIDev.SubClass == 0)
            //    {
            //        xVGADev = xPCIDev;
            //        break;
            //    }
            //}
            //if (xVGADev == null)
            //{
            //    DebugUtil.SendError("Program", "No VGA device found");
            //    goto Klaar;
            //}
            //DebugUtil.SendNumber("Program", "MBI Address", CPU.GetMBIAddress(), 32);

            //var xMBIStruct = ((MultiBootInfoStruct*)(byte*)CPU.GetMBIAddress());
            //DebugUtil.SendNumber("Program", "MBI Addr (2)", (uint)xMBIStruct, 32);
            //DebugUtil.SendNumber("MBI", "Flags", xMBIStruct->Flags, 32);
            //DebugUtil.SendNumber("MBI", "VbeControlInfo", xMBIStruct->VbeControlInfo, 32);
            //DebugUtil.SendNumber("MBI", "VbeModeInfo", xMBIStruct->VbeModeInfo, 32);
            //DebugUtil.SendNumber("MBI", "VbeMode", xMBIStruct->VbeMode, 16);
            //DebugUtil.SendNumber("MBI", "VbeInterfaceSeg", xMBIStruct->VbeInterfaceSeg, 16);
            //DebugUtil.SendNumber("MBI", "VbeInterfaceOff", xMBIStruct->VbeInterfaceOff, 16);
            //DebugUtil.SendNumber("MBI", "VbeInterfaceLen", xMBIStruct->VbeInterfaceLen, 16);



            //Klaar:
            //Console.WriteLine("Ready");
            //while (true) ;

            

            ////SendString("ABBA");

            //Cosmos.
            
            
		}
    }

    [StructLayout(LayoutKind.Explicit)]
    public struct MultiBootInfoStruct
    {
        [FieldOffset(0)]
        public uint Flags;

        [FieldOffset(4)]
        public uint MemLower;

        [FieldOffset(8)]
        public uint MemUpper;

        [FieldOffset(12)]
        public uint BootDevice;

        [FieldOffset(16)]
        public uint CmdLine;

        [FieldOffset(20)]
        public uint ModsCount;

        [FieldOffset(24)]
        public uint ModsAddr;

        [FieldOffset(28)]
        public uint Syms0;

        [FieldOffset(32)]
        public uint Syms1;

        [FieldOffset(36)]
        public uint Syms2;

        [FieldOffset(40)]
        public uint Syms3;

        [FieldOffset(44)]
        public uint MMapLength;

        [FieldOffset(48)]
        public uint MMapAddr;

        [FieldOffset(52)]
        public uint DrivesLength;

        [FieldOffset(56)]
        public uint DrivesAddr;

        [FieldOffset(60)]
        public uint ConfigTable;

        [FieldOffset(64)]
        public uint BootLoaderName;

        [FieldOffset(68)]
        public uint ApmTable;

        [FieldOffset(72)]
        public uint VbeControlInfo;

        [FieldOffset(76)]
        public uint VbeModeInfo;

        [FieldOffset(80)]
        public ushort VbeMode;

        [FieldOffset(82)]
        public ushort VbeInterfaceSeg;

        [FieldOffset(84)]
        public ushort VbeInterfaceOff;

        [FieldOffset(86)]
        public ushort VbeInterfaceLen;
    }
}