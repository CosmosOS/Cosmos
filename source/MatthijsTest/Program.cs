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
using Cosmos.Sys;

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

        [StructLayout(LayoutKind.Explicit, Size=256)]
        public struct VbeInfoBlock
        {
            [FieldOffset(0)]
            public uint VbeSignature;

            [FieldOffset(4)]
            public ushort VbeVersion;

            [FieldOffset(6)]
            public uint OemStringPtr;

            [FieldOffset(10)]
            public uint Capabilities;

            [FieldOffset(14)]
            public uint VideoModePtr;

            [FieldOffset(18)]
            public ushort TotalMemory;

        }

        [StructLayout(LayoutKind.Explicit, Size=256)]
        public struct VbeModeInfoBlock
        {
            [FieldOffset(0)]
            public ushort ModeAttributes;

            [FieldOffset(2)]
            public byte WinAAttributes;

            [FieldOffset(3)]
            public byte WinBAttributes;

            [FieldOffset(4)]
            public ushort WinGranularity;

            [FieldOffset(6)]
            public ushort WinSize;

            [FieldOffset(8)]
            public ushort WinASegment;

            [FieldOffset(10)]
            public ushort WinBSegment;

            [FieldOffset(12)]
            public uint WinFuncPtr;

            [FieldOffset(16)]
            public ushort BytesPerScanLine;

            [FieldOffset(18)]
            public ushort XResolution;

            [FieldOffset(20)]
            public ushort YResolution;

            [FieldOffset(22)]
            public byte XCharSize;

            [FieldOffset(23)]
            public byte YCharSize;

            [FieldOffset(24)]
            public byte NumberOfPlanes;

            [FieldOffset(25)]
            public byte BitsPerPixel;

            [FieldOffset(26)]
            public byte NumberOfBanks;

            [FieldOffset(27)]
            public byte MemoryModel;

            [FieldOffset(28)]
            public byte BankSize;

            [FieldOffset(29)]
            public byte NumberOfImagePages;

            [FieldOffset(30)]
            public byte Reserved;

            [FieldOffset(31)]
            public byte RedMaskSize;

            [FieldOffset(32)]
            public byte RedFieldPosition;

            [FieldOffset(33)]
            public byte GreenMaskSize;

            [FieldOffset(34)]
            public byte GreenFieldPosition;

            [FieldOffset(35)]
            public byte BlueMaskSize;

            [FieldOffset(36)]
            public byte BlueFieldPosition;

            [FieldOffset(37)]
            public byte RsvdMaskSize;

            [FieldOffset(38)]
            public byte RsvdFieldPosition;

            [FieldOffset(39)]
            public byte DirectColorMode;

            [FieldOffset(40)]
            public uint PhysBasePtr;

            [FieldOffset(44)]
            public uint Reserved1;

            [FieldOffset(48)]
            public ushort Reserved2;

            [FieldOffset(50)]
            public ushort LinBytesPerScanLine;

            [FieldOffset(52)]
            public byte BnkNumberOfImagePages;

            [FieldOffset(53)]
            public byte LinNumberOfImagePages;

            [FieldOffset(54)]
            public byte LinRedMaskSize;

            [FieldOffset(55)]
            public byte LinRedFieldPosition;

            [FieldOffset(56)]
            public byte LinGreenMaskSize;

            [FieldOffset(57)]
            public byte LinGreenFieldPosition;

            [FieldOffset(58)]
            public byte LinBlueMaskSize;

            [FieldOffset(59)]
            public byte LinBlueFieldPosition;

            [FieldOffset(60)]
            public byte LinRsvdMaskSize;

            [FieldOffset(61)]
            public byte LinRsvdFieldPosition;

            [FieldOffset(62)]
            public uint MaxPixelClock;
            // end
        }

        public static unsafe void Init()
        {

            var xInit = false;
            if (xInit)
            {
                var xBoot = new Cosmos.Sys.Boot();
                xBoot.Execute(true);

                var xDirs = Directory.GetDirectories("/0/");
                Console.WriteLine("Directories: " + xDirs.Length);
                for (int i = 0; i < xDirs.Length; i++)
                {
                    Console.WriteLine(xDirs[i]);
                }
                var xFiles = Directory.GetFiles("/0/");
                Console.WriteLine("Files: " + xFiles.Length);
                for (int i = 0; i < xFiles.Length; i++)
                {
                    Console.Write("  ");
                    Console.WriteLine(xFiles[i]);
                }
            }
            else
            {
                DebugUtil.SendMessage("Program", "Start");
                DebugUtil.SendNumber("Program", "VbeMode", BootInfo.GetVbeMode(), 16);
                DebugUtil.SendNumber("Program", "VbeModeInfo", BootInfo.GetVbeModeInfoAddr(), 32);
                DebugUtil.SendNumber("Program", "VbeControlInfo", BootInfo.GetVbeControlInfoAddr(), 32);
                var xVbeModeInfoBlock = (VbeModeInfoBlock*)BootInfo.GetVbeModeInfoAddr();
                var xVbeInfoBlock = (VbeInfoBlock*)BootInfo.GetVbeControlInfoAddr();

                DebugUtil.SendNumber("VbeInfoBlock", "VbeSignature", xVbeInfoBlock->VbeSignature, 32);
                DebugUtil.SendNumber("VbeInfoBlock", "VbeVersion", xVbeInfoBlock->VbeVersion, 16);
                DebugUtil.SendNumber("VbeInfoBlock", "OemStringPtr", xVbeInfoBlock->OemStringPtr, 32);
                DebugUtil.SendNumber("VbeInfoBlock", "Capabilities", xVbeInfoBlock->Capabilities, 32);
                DebugUtil.SendNumber("VbeInfoBlock", "VideoModePtr", xVbeInfoBlock->VideoModePtr, 32);
                DebugUtil.SendNumber("VbeInfoBlock", "TotalMemory", xVbeInfoBlock->TotalMemory, 16);
                //
                DebugUtil.SendNumber("VbeModeInfoBlock", "ModeAttributes", xVbeModeInfoBlock->ModeAttributes, 16);
                DebugUtil.SendNumber("VbeModeInfoBlock", "WinAAttributes", xVbeModeInfoBlock->WinAAttributes, 8);
                DebugUtil.SendNumber("VbeModeInfoBlock", "WinBAttributes", xVbeModeInfoBlock->WinBAttributes, 8);
                DebugUtil.SendNumber("VbeModeInfoBlock", "WinGranularity", xVbeModeInfoBlock->WinGranularity, 16);
                DebugUtil.SendNumber("VbeModeInfoBlock", "WinSize", xVbeModeInfoBlock->WinSize, 16);
                DebugUtil.SendNumber("VbeModeInfoBlock", "WinASegment", xVbeModeInfoBlock->WinASegment, 16);
                DebugUtil.SendNumber("VbeModeInfoBlock", "WinBSegment", xVbeModeInfoBlock->WinBSegment, 16);
                DebugUtil.SendNumber("VbeModeInfoBlock", "WinFuncPtr", xVbeModeInfoBlock->WinFuncPtr, 32);
                DebugUtil.SendNumber("VbeModeInfoBlock", "BytesPerScanLine", xVbeModeInfoBlock->BytesPerScanLine, 16);
                DebugUtil.SendNumber("VbeModeInfoBlock", "XResolution", xVbeModeInfoBlock->XResolution, 16);
                DebugUtil.SendNumber("VbeModeInfoBlock", "YResolution", xVbeModeInfoBlock->YResolution, 16);
                DebugUtil.SendNumber("VbeModeInfoBlock", "XCharSize", xVbeModeInfoBlock->XCharSize, 8);
                DebugUtil.SendNumber("VbeModeInfoBlock", "YCharSize", xVbeModeInfoBlock->YCharSize, 8);
                DebugUtil.SendNumber("VbeModeInfoBlock", "NumberOfPlanes", xVbeModeInfoBlock->NumberOfPlanes, 8);
                DebugUtil.SendNumber("VbeModeInfoBlock", "BitsPerPixel", xVbeModeInfoBlock->BitsPerPixel, 8);
                DebugUtil.SendNumber("VbeModeInfoBlock", "NumberOfBanks", xVbeModeInfoBlock->NumberOfBanks, 8);
                DebugUtil.SendNumber("VbeModeInfoBlock", "MemoryModel", xVbeModeInfoBlock->MemoryModel, 8);
                DebugUtil.SendNumber("VbeModeInfoBlock", "BankSize", xVbeModeInfoBlock->BankSize, 8);
                DebugUtil.SendNumber("VbeModeInfoBlock", "NumberOfImagePages", xVbeModeInfoBlock->NumberOfImagePages, 8);
                DebugUtil.SendNumber("VbeModeInfoBlock", "Reserved", xVbeModeInfoBlock->Reserved, 8);
                DebugUtil.SendNumber("VbeModeInfoBlock", "RedMaskSize", xVbeModeInfoBlock->RedMaskSize, 8);
                DebugUtil.SendNumber("VbeModeInfoBlock", "RedFieldPosition", xVbeModeInfoBlock->RedFieldPosition, 8);
                DebugUtil.SendNumber("VbeModeInfoBlock", "GreenMaskSize", xVbeModeInfoBlock->GreenMaskSize, 8);
                DebugUtil.SendNumber("VbeModeInfoBlock", "GreenFieldPosition", xVbeModeInfoBlock->GreenFieldPosition, 8);
                DebugUtil.SendNumber("VbeModeInfoBlock", "BlueMaskSize", xVbeModeInfoBlock->BlueMaskSize, 8);
                DebugUtil.SendNumber("VbeModeInfoBlock", "BlueFieldPosition", xVbeModeInfoBlock->BlueFieldPosition, 8);
                DebugUtil.SendNumber("VbeModeInfoBlock", "RsvdMaskSize", xVbeModeInfoBlock->RsvdMaskSize, 8);
                DebugUtil.SendNumber("VbeModeInfoBlock", "RsvdFieldPosition", xVbeModeInfoBlock->RsvdFieldPosition, 8);
                DebugUtil.SendNumber("VbeModeInfoBlock", "DirectColorMode", xVbeModeInfoBlock->DirectColorMode, 8);
                DebugUtil.SendNumber("VbeModeInfoBlock", "PhysBasePtr", xVbeModeInfoBlock->PhysBasePtr, 32);
                DebugUtil.SendNumber("VbeModeInfoBlock", "Reserved1", xVbeModeInfoBlock->Reserved1, 32);
                DebugUtil.SendNumber("VbeModeInfoBlock", "Reserved2", xVbeModeInfoBlock->Reserved2, 16);
                DebugUtil.SendNumber("VbeModeInfoBlock", "LinBytesPerScanLine", xVbeModeInfoBlock->LinBytesPerScanLine, 16);
                DebugUtil.SendNumber("VbeModeInfoBlock", "BnkNumberOfImagePages", xVbeModeInfoBlock->BnkNumberOfImagePages, 8);
                DebugUtil.SendNumber("VbeModeInfoBlock", "LinNumberOfImagePages", xVbeModeInfoBlock->LinNumberOfImagePages, 8);
                DebugUtil.SendNumber("VbeModeInfoBlock", "LinRedMaskSize", xVbeModeInfoBlock->LinRedMaskSize, 8);
                DebugUtil.SendNumber("VbeModeInfoBlock", "LinRedFieldPosition", xVbeModeInfoBlock->LinRedFieldPosition, 8);
                DebugUtil.SendNumber("VbeModeInfoBlock", "LinGreenMaskSize", xVbeModeInfoBlock->LinGreenMaskSize, 8);
                DebugUtil.SendNumber("VbeModeInfoBlock", "LinGreenFieldPosition", xVbeModeInfoBlock->LinGreenFieldPosition, 8);
                DebugUtil.SendNumber("VbeModeInfoBlock", "LinBlueMaskSize", xVbeModeInfoBlock->LinBlueMaskSize, 8);
                DebugUtil.SendNumber("VbeModeInfoBlock", "LinBlueFieldPosition", xVbeModeInfoBlock->LinBlueFieldPosition, 8);
                DebugUtil.SendNumber("VbeModeInfoBlock", "LinRsvdMaskSize", xVbeModeInfoBlock->LinRsvdMaskSize, 8);
                DebugUtil.SendNumber("VbeModeInfoBlock", "LinRsvdFieldPosition", xVbeModeInfoBlock->LinRsvdFieldPosition, 8);
                DebugUtil.SendNumber("VbeModeInfoBlock", "MaxPixelClock", xVbeModeInfoBlock->MaxPixelClock, 32);

            }
            //MyStruct xTest;
            //DoTest(out xTest);
            //Console.WriteLine("Value1: " + xTest.Value1);
            //Console.WriteLine("Value2: " + xTest.Value2);

            //var xFiles = Directory.GetFiles(@"/0");
            //Console.WriteLine("Iteraten: " + xFiles.Length);
            //for (int i = 0; i < xFiles.Length; i++)
            //{
            //    Console.WriteLine(xFiles[i]);
            //}

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

    //public static class DebugUtil
    //{
    //    public static void Write(string data)
    //    {
    //        for (int i = 0; i < data.Length; i++)
    //        {
    //            Write(data[i]);
    //        }
    //    }

    //    private static void Write(char data)
    //    {
    //        Serial.WriteSerial(0, (byte)data);
    //    }

    //    public static void WriteUIntAsHex(uint data)
    //    {
    //        Write("0x");
    //        var xTemp = "0123456789ABCDEF";
    //        Write(xTemp[((int)((data >> 28) & 0xF))]);
    //        Write(xTemp[((int)((data >> 24) & 0xF))]);
    //        Write(xTemp[((int)((data >> 20) & 0xF))]);
    //        Write(xTemp[((int)((data >> 16) & 0xF))]);
    //        Write(xTemp[((int)((data >> 12) & 0xF))]);
    //        Write(xTemp[((int)((data >> 8) & 0xF))]);
    //        Write(xTemp[((int)((data >> 4) & 0xF))]);
    //        Write(xTemp[((int)((data) & 0xF))]);
    //    }

    //    public static void WriteLine(string data)
    //    {
    //        Write(data);
    //        Write("\r\n");
    //    }
    //}
}