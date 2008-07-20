using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Security.Cryptography;
using Cosmos.Build.Windows;
using Cosmos.FileSystem.Ext2;
using Cosmos.Hardware;

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

        public static void Init() {
            Cosmos.Sys.Boot.Default();
            var xStorage = Cosmos.Hardware.Device.FindFirst(Device.DeviceType.Storage) as BlockDevice;
            if (xStorage == null) {
                Console.WriteLine("ERROR: StorageDevice not found!");
                return;
            }
            var xExt2 = new Ext2(xStorage);
            var xDirectoryListing = xExt2.GetDirectoryListing(xExt2.RootId);
            bool xFileWritten = false;
            if (xDirectoryListing == null) {
                Console.WriteLine("No DirectoryListing!");
            } else {
                Console.Write("Directory entries count: ");
                Console.WriteLine(xDirectoryListing.Length.ToString());
                for (int i = 0; i < xDirectoryListing.Length; i++) {
                    Console.Write(((uint)xDirectoryListing[i].Id).ToString());
                    Console.Write("|");
                    Console.Write(xDirectoryListing[i].Name);
                    if (xDirectoryListing[i].IsDirectory) {
                        Console.WriteLine("/");
                    } else {
                        Console.Write("|");
                        Console.WriteLine(((uint)xDirectoryListing[i].Size).ToString());
                        if (!xFileWritten) {
                            xFileWritten = true;
                            byte[] xBuff = new byte[xExt2.BlockSize];

                            if (!xExt2.ReadBlock(xDirectoryListing[i].Id,
                                                 0,
                                                 xBuff)) {
                                Console.WriteLine("Error while reading file contents!");
                                continue;
                            }
                            var xChars = new char[xDirectoryListing[i].Size];
                            for (int j = 0; j < xBuff.Length; j++) {
                                xChars[j] = (char)xBuff[j];
                            }
                            Console.Write(new string(xChars));
                        }
                    }
                }
            }
            Console.WriteLine("Shutting down!");
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