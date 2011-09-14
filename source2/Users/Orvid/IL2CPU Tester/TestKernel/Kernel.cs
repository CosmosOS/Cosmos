#pragma warning disable 162 
// The compiler doesn't like the fact that true == true might not necessarily be true.
using System;
using System.Collections.Generic;
using System.Text;
using Sys = Cosmos.System;

namespace TestKernel
{
    public class Kernel : Sys.Kernel
    {
        public Kernel() : base()
        {
            this.ClearScreen = false;
        }
        protected override void BeforeRun() { }

        protected override void Run()
        {
            #region Byte Conversions

            #endregion


            //byte r = 255;
            //sbyte r = -128;
            //char r = '\u0127';
            //ushort r = 65535;
            //short r = -32768;
            //uint r = 4294967295;
            //int r = -2147483648;
            //ulong r = 18446744073709551615;
            //long r = 9223372036854775807;
            float r = 123.7f;
            //double r = 123.7;
            Console.WriteLine("Just one more test.");
            Console.WriteLine("Raw Byte: " + r.ToString());
            Console.WriteLine("Byte->Byte: " + (unchecked((byte)r)).ToString());
            Console.WriteLine("Byte->SByte: " + (unchecked((sbyte)r)).ToString());
            Console.WriteLine("Byte->Char: " + (unchecked((char)r)).ToString());
            Console.WriteLine("Byte->UShort: " + (unchecked((ushort)r)).ToString());
            Console.WriteLine("Byte->Short: " + (unchecked((short)r)).ToString());
            Console.WriteLine("Byte->UInt: " + (unchecked((uint)r)).ToString());
            Console.WriteLine("Byte->Int: " + (unchecked((int)r)).ToString());
            Console.WriteLine("Byte->ULong: " + (unchecked((ulong)r)).ToString());
            Console.WriteLine("Byte->Long: " + (unchecked((long)r)).ToString());
            Console.WriteLine("Byte->Float: " + (unchecked((float)r)).ToString());
            Console.WriteLine("Byte->Double: " + (unchecked((double)r)).ToString());
            if (unchecked((decimal)r) != 123.7m)
            {

            }

            while (true) { } // Prevent the check from being run again, and display results.
        }

        public static void WriteError(string s)
        {
            Console.WriteLine("Error: " + s);
        }
    }
}
#pragma warning restore 162