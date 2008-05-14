using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Cosmos.Build.Windows;
using System.Collections;
using System.Security.Cryptography;

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

        [ManifestResourceStream(ResourceName="MatthijsTest.Test.txt")]
        private static readonly byte[] TheManifestResource;

        public static void Init()
        {
            Console.Clear();
            MemoryStream xMS = new MemoryStream(TheManifestResource, false);
            Console.Write("Length: ");
            Console.WriteLine(((int)xMS.Length).ToString());
            SHA256Managed xSHA = new SHA256Managed();
            var xHash = xSHA.ComputeHash(xMS);
            Console.Write("0x");
            for (int i = 0; i < xHash.Length; i++) { 
            PrintHex(xHash[i]);}
            Console.WriteLine("");
        }

        private static string SingleDigitToHex(byte d)
        {
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

        public static void PrintHex(byte aByte) {
            Console.Write(SingleDigitToHex((byte)(aByte / 16)));
            Console.Write(SingleDigitToHex((byte)(aByte & 0xF)));
        }
    }
}
