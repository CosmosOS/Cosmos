using System;
using Cosmos.Build.Windows;

namespace DokuTest
{
    class Program
    {
        #region Cosmos Builder logic
        // Most users wont touch this. This will call the Cosmos Build tool
        [STAThread]
        static void Main(string[] args) {
            BuildUI.Run();
        }
        #endregion

        public static void Init()
        {
            var xBoot = new Cosmos.Sys.Boot();
            xBoot.Execute();
            /*
            Console.ForegroundColor=ConsoleColor.Black;
            Console.WriteLine("011000000111011011110000101101101101010110000011010100111110010010110110000000");
            Console.Write("0100010011110");
            Console.ForegroundColor=ConsoleColor.Green;
            Console.Write("111011000010");
            Console.ForegroundColor=ConsoleColor.Black;
            Console.WriteLine("01000100110111110111111011010010111110101001001100010");
            Console.Write("0010001100");
            Console.ForegroundColor=ConsoleColor.Green;
            Console.Write("100101101010");
            Console.ForegroundColor=ConsoleColor.Black;
            Console.WriteLine("10011010001001000101101110100110101111010001010110110010");
            Console.Write("11011100");
            Console.ForegroundColor=ConsoleColor.Green;
            Console.Write("0010100000100100");
            Console.ForegroundColor=ConsoleColor.Black;
            Console.WriteLine("101010011000111001100100001011110001000001000010111111");
            Console.Write("0110011");
            Console.ForegroundColor=ConsoleColor.Green;
            Console.Write("0110101");
            Console.ForegroundColor=ConsoleColor.Black;
            Console.WriteLine("0010010101110101011011110000010111100001110001001011111111100111");
            Console.Write("000000");
            Console.ForegroundColor=ConsoleColor.Green;
            Console.Write("100010101");
            Console.ForegroundColor=ConsoleColor.Black;
            Console.Write("0110");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("111000000");
            Console.ForegroundColor = ConsoleColor.Black;
            Console.Write("11");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("1001011");
            Console.ForegroundColor = ConsoleColor.Black;
            Console.Write("00");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("0100");
            Console.ForegroundColor = ConsoleColor.Black;
            Console.Write("0110");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("11010");
            Console.ForegroundColor = ConsoleColor.Black;
            Console.Write("110");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("0011110101");
            Console.ForegroundColor = ConsoleColor.Black;
            Console.Write("10");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("1100001");
            Console.ForegroundColor = ConsoleColor.Black;
            Console.Write("10111");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("1001001");
            Console.ForegroundColor = ConsoleColor.Black;
            Console.Write("100");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("10111011101");
            Console.ForegroundColor = ConsoleColor.Black;
            Console.Write("0");
            Console.ForegroundColor = ConsoleColor.Green;*/
            int i = 0;
            while (true)
            {
                Console.WriteLine(i);
                ++i;
            }
            Console.Read();
            Console.WriteLine("*** COSMOS Operating System - Doku's Test Suite ***");
            Console.WriteLine("*** Ensoniq AudioPCI 1370 testing ***");
            ES1370.test();
            Console.WriteLine("Shutting down computer");
            Cosmos.Sys.Deboot.ShutDown();            
        }
    }
}