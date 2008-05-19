using System;
using System.IO;
using System.IO.Compression;
using System.Security.Cryptography;
using Cosmos.Build.Windows;
using ICSharpCode.SharpZipLib.Zip.Compression;
using ICSharpCode.SharpZipLib.Zip.Compression.Streams;
using Huffman;

namespace MatthijsTest
{

    public class Program
    {
        #region Cosmos Builder logic

        // Most users wont touch this. This will call the Cosmos Build tool
        [STAThread]
        private static void Main(string[] args)
        {
            BuildUI.Run();
        }

        #endregion

        [ManifestResourceStream(ResourceName = "MatthijsTest.Test.txt")]
        private static readonly byte[] TheManifestResource;
        [ManifestResourceStream(ResourceName = "MatthijsTest.Test.txt.gz")]
        private static readonly byte[] TheManifestResourceZipped;

        public static unsafe void Init()
        {
            Console.Clear();

            MemoryStream xMS = new MemoryStream(TheManifestResource, false);
            Console.Write("Length: ");
            Console.WriteLine(((int)xMS.Length).ToString());

            var xHA = new HuffmanAlgorithm();
            xHA.Shrink(
            xSHA.Initialize(); 
        }

        private static string SingleDigitToHex(byte d)
        {
            d &= 0xF;
            switch (d)
            {
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

        public static void PrintHex(byte aByte)
        {
            Console.Write(SingleDigitToHex((byte)(aByte / 16)));
            Console.Write(SingleDigitToHex((byte)(aByte & 0xF)));
        }

        public static void PrintHex(uint aUint)
        {
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
