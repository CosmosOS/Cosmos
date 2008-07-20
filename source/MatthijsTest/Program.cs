using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Security.Cryptography;
using Cosmos.Build.Windows;
using Cosmos.FileSystem.Ext2;
using Cosmos.Hardware;
using Cosmos.Hardware.Storage.ATA;
using ICSharpCode.SharpZipLib.Zip.Compression;
using ICSharpCode.SharpZipLib.Zip.Compression.Streams;
using Huffman;

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

        //[ManifestResourceStream(ResourceName = "MatthijsTest.Test.txt")]
        //private static readonly byte[] TheManifestResource;
        //[ManifestResourceStream(ResourceName = "MatthijsTest.Test.txt.gz")]
        //private static readonly byte[] TheManifestResourceZipped;

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
            if (xDirectoryListing == null) {
                Console.WriteLine("No DirectoryListing!");
            } else {
                Console.Write("Directory entries count: ");
                Console.WriteLine(xDirectoryListing.Length.ToString());
                for (int i = 0; i < xDirectoryListing.Length; i++) {
                    Console.Write(xDirectoryListing[i].Name);
                    if (xDirectoryListing[i].IsDirectory) {
                        Console.WriteLine("/");
                        if(xDirectoryListing[i].Id != xExt2.RootId) {
                            var xDirListing2 = xExt2.GetDirectoryListing(xDirectoryListing[i].Id);
                            for(int j = 0; j<xDirListing2.Length;j++) {
                                Console.Write("    ");
                                Console.Write(xDirListing2[j].Name);
                                if (xDirListing2[j].IsDirectory) {
                                    Console.WriteLine("/");
                                }else{Console.WriteLine("");}

                            }
                        }
                    } else {
                        Console.WriteLine("");
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