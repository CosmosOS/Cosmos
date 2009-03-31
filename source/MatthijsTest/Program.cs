using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using Cosmos.Compiler.Builder;
using Cosmos.Sys.FileSystem;
using Cosmos.Sys.FileSystem.Ext2;
using Cosmos.Hardware;
using Cosmos.Kernel;
using Cosmos.Sys;
using Cosmos.Sys.Network;
using Mono.Terminal;
using DebugUtil=Cosmos.Hardware.DebugUtil;
using Indy.IL2CPU;

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

        public static unsafe void Init() {
            bool xTest = false;
            if (xTest) {
                var xBoot = new Cosmos.Sys.Boot();
                xBoot.Execute();
            }
            Console.WriteLine("Hello, World!");
            Cosmos.Sys.Deboot.ShutDown();
            
        }

        private static void HandleSendBroadCast(string command) {
            var xUDP = new Cosmos.Sys.Network.UDPPacket(// Use a different port so it does not conflict wtih listener since we
                    // are using the same IP on host for testing
                    0x0A00020F,
                    32001 // 10.0.2.15
                    ,
                    0xFFFFFFFF,
                    9765 // 255.255.255.255, Broadcast
                    ,
                    new byte[] {
                                       65, 66, 67, 68, 69, 70, 71, 72, 73, 74, 75, 76, 77, 78, 79, 80, 81, 82, 83, 84, 85, 86, 87, 88, 89, 90
                               });
            var xEthernet = new EthernetPacket(xUDP.GetData()
                , 0x525400123457, 0xFFFFFFFFFFFF
                , EthernetPacket.PacketType.IP);

            var xNICs = Cosmos.Hardware.Network.NetworkDevice.NetworkDevices;
            var xNIC = xNICs[0];
            xNIC.Enable();
            var xBytes = xEthernet.GetData();
            DebugUtil.WriteBinary("RTLTest", "Prepare to send packet", xBytes);
            xNIC.QueueBytes(xBytes);
        }

        private static void HandleType(string command) {
            string xFile = command.Substring(5);
            Console.WriteLine(File.ReadAllText("/0/" +xFile));
        }

        private static void HandleDir() {
            var xItems = Directory.GetDirectories("/0");
            for(int i = 0; i<xItems.Length;i++) {
                Console.Write("./");
                Console.Write(xItems[i]);
                Console.WriteLine("/");
            }
            xItems = Directory.GetFiles("/0");
            for(int i = 0; i<xItems.Length;i++) {
                Console.Write("./");
                Console.WriteLine(xItems[i]);
            }
        }

        public static int TestMethodNoParams()
        {
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
