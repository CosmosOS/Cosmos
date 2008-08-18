using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Security.Cryptography;
using Cosmos.Build.Windows;
using Cosmos.Sys.FileSystem;
using Cosmos.Sys.FileSystem.Ext2;
using Cosmos.Hardware;
using Cosmos.Kernel;
using Cosmos.Sys;
using Cosmos.Sys.Network;
using Mono.Terminal;
using DebugUtil=Cosmos.Hardware.DebugUtil;

namespace MatthijsTest {
    public class Program {
        #region Cosmos Builder logic

        // Most users wont touch this. This will call the Cosmos Build tool
        [STAThread]
        private static void Main(string[] args) {
            //Init();
            BuildUI.Run();
        }

        #endregion

        private static ushort HostToNetwork(ushort aValue) {
            return (ushort)((aValue << 8) | ((aValue >> 8) & 0xFF));
        }

        private static uint HostToNetwork(uint aValue) {
            return (uint)(((HostToNetwork((ushort)aValue) & 0xffff) << 0x10) | (HostToNetwork((ushort)(aValue >> 0x10)) & 0xffff));
        }

        private static void ProcessFile(Ext2 aExt2,
                                        FilesystemEntry aEntry) {
            byte[] xTempBuff = new byte[aExt2.BlockSize];
            byte[] xTotalBuff = new byte[aEntry.Size];
            for (uint i = 0; i < (xTotalBuff.Length / xTempBuff.Length); i++) {
                if (!aExt2.ReadBlock(aEntry.Id,
                                     i,
                                     xTempBuff)) {
                    Console.Write("Error while processing file! (");
                    Console.Write(((uint)aEntry.Id).ToString());
                    Console.WriteLine(")");
                    return;
                }
                int xCurLength = xTotalBuff.Length % xTempBuff.Length;
                if (xCurLength == 0) {
                    xCurLength = xTempBuff.Length;
                }
                Array.Copy(xTempBuff,
                           0,
                           xTotalBuff,
                           i * xTempBuff.Length,
                           xCurLength);
                if ((i + 1) == (xTotalBuff.Length / xTempBuff.Length)) {
                    break;
                }
            }
            if (aEntry.Size >= 512) {
                var xBlockCount = ((uint)(aEntry.Size)) / 512;
                if (aEntry.Size % 512 > 0) {
                    xBlockCount ++;
                }
                for (var i = 0; i < xBlockCount; i++) {
                    var xCurBlockSize = 512;
                    var xCharBuffer = new char[512];
                    if (i == (xBlockCount - 1)) {
                        if ((xTotalBuff.Length - (xBlockCount * 512)) < 512) {
                            xCurBlockSize = (int)(xTotalBuff.Length - (uint)(xBlockCount * 512));
                        }
                    }
                    for (int j = 0; j < xCurBlockSize; j++) {
                        xCharBuffer[j] = (char)xTotalBuff[(i * 512) + j];
                    }
                    Console.Write("Printing file ");
                    Console.Write(aEntry.Name);
                    Console.Write(", Block ");
                    Console.Write(i.ToString());
                    Console.Write(" of ");
                    Console.Write(xBlockCount.ToString());
                    Console.WriteLine(":");
                    Console.WriteLine(new string(xCharBuffer,
                                                 0,
                                                 xCurBlockSize));
                    Console.Write("Press a key to continue");
                    Console.ReadLine();
                }
            } else {
                Console.WriteLine("Not enough data");
            }
        }

        public delegate void TestDelegate(int aValue,
                                          ref bool aResult);

        public static void Handler1(int aValue,
                                    ref bool aResult) {
            Console.Write("Result = ");
            if (!aResult) {
                Console.WriteLine("false");
                aResult = true;
            } else {
                Console.WriteLine("true");
            }
        }

        [ManifestResourceStream(ResourceName = "MatthijsTest.TestV86MExecution")]
        public static readonly byte[] TestV86MExecution;

        public static void DoTest() {
            Console.WriteLine("Original WriteLine");
        }

        public class TestClass {
            public int ThrowException2() {
                ThrowException();
                return 1;
            }

            public int ThrowException() {
                throw new Exception("Test Error");
            }
        }

        private static void MyMethod() {
            Console.WriteLine("From static method");
            Console.ReadLine();
        }

        public class TestClassEvents {
            public void Test() {
                Console.WriteLine("From instance method");
            }
        }

        //private static event Action TestEvent;
        public static void Init() {
            bool xTest = true;
            if (xTest) {
                var xBoot = new Cosmos.Sys.Boot();
                xBoot.Execute();
            }
            var xUDP = new Cosmos.Sys.Network.UDPPacket(// Use a different port so it does not conflict wtih listener since we
                // are using the same IP on host for testing
                0x0A00020F,
                32001 // 10.0.2.15
                ,
                0xFFFFFFFF,
                32000 // 255.255.255.255, Broadcast
                ,
                new byte[] {0x16, 0x16, 0x16, 0x16, 0x16, 0x16, 0x16, 0x16, 0x16, 0x16, 0x16, 0x16, 0x16, 0x16, 0x16, 0x16, 0x16, 0x16, 0x16, 0x16, 0x16, 0x16, 0x16, 0x16, 0x16, 0x16, 0x16, 0x16, 0x16, 0x16, 0x16, 0x16, 0x16, 0x16, 0x16, 0x16, 0x16, 0x16, 0x16, 0x16, 0x16, 0x16, 0x16, 0x16, 0x16});
            var xEthernet = new EthernetPacket(xUDP.GetData(),
                                               0x525400123457,
                                               0xFFFFFFFFFFFF,
                                               EthernetPacket.PacketType.IP);

            Cosmos.Hardware.Network.Devices.RTL8139.RTL8139.DebugOutput = false;
            var xNICs = Cosmos.Hardware.Network.Devices.RTL8139.RTL8139.FindAll();
            var xNIC = xNICs[0];

            Console.WriteLine(xNIC.Name);
            Console.WriteLine("Revision: " + xNIC.HardwareRevision);
            Console.WriteLine("MAC: " + xNIC.MACAddress);

            Console.WriteLine("Enabling network card.");
            xNIC.Enable();
            xNIC.InitializeDriver();

            Console.WriteLine("Sending bytes.");
            var xBytes = xEthernet.GetData();
            System.Diagnostics.Debugger.Break();
            xNIC.TransmitBytes(xBytes);
            Console.WriteLine("Kernel done doing it's thing...");
            do {
                Console.WriteLine(Console.ReadLine());
                while (xNIC.mBuffer.Count > 0) {
                    var xPacket = xNIC.mBuffer.Dequeue();
                    ushort x = (ushort)((xPacket[0x12] << 8) | xPacket[0x13]);
                    DebugUtil.SendNumber("Test",
                                         "Identification",
                                         x,
                                         16);
                }
            } while (true);
        }

        public static int TestMethodNoParams() {
            return 23;
        }

        public static int TestMethodOneParams(int theValue) {
            return theValue * 2;
        }

        public static int TestMethodTwoParams(int theValue,
                                              int theValue2) {
            return theValue + theValue2;
        }

        public static int TestMethodThreeParams(int theValue,
                                                int theValue2,
                                                int theValue3) {
            return theValue + theValue2 + theValue3;
        }

        public static int TestMethodComplicated(ulong aValue,
                                                bool atest) {
            return 7356;
        }

        public static void Handler1(object sender,
                                    EventArgs e) {
            if (sender == null) {
                Console.WriteLine("Sender is null");
            } else {
                Console.WriteLine("Sender is not null");
            }
        }

        public static void Handler2(object sender,
                                    EventArgs e) {
            if (sender == null) {
                Console.WriteLine("Sender is null");
            } else {
                Console.WriteLine("Sender is not null");
            }
        }

        private static EventHandler theEvent;

        private static string SingleDigitToHex(byte d) {
            d &= 0xF;
            switch (d) {
                case 0:
                    return "0";
                case 1:
                    return "1";
                case 2:
                    return "2";
                case 3:
                    return "3";
                case 4:
                    return "4";
                case 5:
                    return "5";
                case 6:
                    return "6";
                case 7:
                    return "7";
                case 8:
                    return "8";
                case 9:
                    return "9";
                case 10:
                    return "A";
                case 11:
                    return "B";
                case 12:
                    return "C";
                case 13:
                    return "D";
                case 14:
                    return "E";
                case 15:
                    return "F";
            }
            return " ";
        }

        public static void PrintHex(byte aByte) {
            Console.Write(SingleDigitToHex((byte)(aByte / 16)));
            Console.Write(SingleDigitToHex((byte)(aByte & 0xF)));
        }

        public static void PrintHex(uint aUint) {
            Console.Write(SingleDigitToHex((byte)(aUint >> 28)));
            Console.Write(SingleDigitToHex((byte)(aUint >> 24)));
            Console.Write(SingleDigitToHex((byte)(aUint >> 20)));
            Console.Write(SingleDigitToHex((byte)(aUint >> 16)));
            Console.Write(SingleDigitToHex((byte)(aUint >> 12)));
            Console.Write(SingleDigitToHex((byte)(aUint >> 8)));
            Console.Write(SingleDigitToHex((byte)(aUint >> 4)));
            Console.Write(SingleDigitToHex((byte)(aUint & 0xF)));
        }
    }
}