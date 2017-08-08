using System;
using System.Collections.Generic;
//using Math = Cosmos.System_Plugs.MathImpl;
using System.Text;
using Sys = Cosmos.System;
using Orvid.Graphics;

namespace StructTest
{
    public class Kernel : Sys.Kernel
    {

        public Kernel()
        {
            base.ClearScreen = true;
            
        }

        protected override void BeforeRun()
        {
            Console.WriteLine("Hello, and welcome to the TestOS by Orvid.");
            Console.WriteLine("To see a list of supported commands, type '?'");
        }


        public void DoRun()
        {
            this.Run();
        }

        protected override void Run()
        {
            //Cosmos.Core.RAM.Initialize();
            //double d = 40.23;
            //double d2 = 93.210;
            //double d3 = -412.569;
            //double d4 = 0.45912;
            //Console.WriteLine("Dividing 40.23 by 40.23: " + (d / d).ToString() + " (1)");
            //Console.WriteLine("Dividing 40.23 by 93.210: " + (d / d2).ToString() + " (0.431606050852913)");
            //Console.WriteLine("Dividing 40.23 by -412.569: " + (d / d3).ToString() + " (-0.097510961802753)");
            //Console.WriteLine("Dividing 40.23 by 0.45912: " + (d / d4).ToString() + " (87.6241505488761)");
            //Console.WriteLine("Dividing 93.210 by 40.23: " + (d2 / d).ToString() + " (2.31692766592095)");
            //Console.WriteLine("Dividing 93.210 by 93.210: " + (d2 / d2).ToString() + " (1)");
            //Console.WriteLine("Dividing 93.210 by -412.569: " + (d2 / d3).ToString() + " (-0.22592584513136)");
            //Console.WriteLine("Dividing 93.210 by 0.45912: " + (d2 / d4).ToString() + " (203.018818609514)");
            //Console.WriteLine("Dividing -412.569 by 40.23: " + (d3 / d).ToString() + " (-10.255257270693)");
            //Console.WriteLine("Dividing -412.569 by 93.210: " + (d3 / d2).ToString() + " (-4.42623109108465)");
            //Console.WriteLine("Dividing -412.569 by -412.569: " + (d3 / d3).ToString() + " (1)");
            //Console.WriteLine("Dividing -412.569 by 0.45912: " + (d3 / d4).ToString() + " (-898.608207004705)");
            //Console.WriteLine("Dividing 0.45912 by 40.23: " + (d4 / d).ToString() + " (0.0114123788217748)");
            //Console.WriteLine("Dividing 0.45912 by 93.210: " + (d4 / d2).ToString() + " (0.00492565175410364)");
            //Console.WriteLine("Dividing 0.45912 by -412.569: " + (d4 / d3).ToString() + " (-0.00111283203536863)");
            //Console.WriteLine("Dividing 0.45912 by 0.45912: " + (d4 / d4).ToString() + " (1)");


            //Cosmos.Hardware.Drivers.PCI.Video.VMWareSVGAII v = new Cosmos.Hardware.Drivers.PCI.Video.VMWareSVGAII();
            //v.Clear(0x00000000);
            //v.Fill(30, 30, 30, 30, 0x88888888);
            //v.Update(0, 0, 800, 600);
            //Monitor m = new Monitor();
            //byte r = 255;
            //sbyte r = -128;
            //char r = '\u0127';
            //ushort r = 65535;
            //short r = -32768;
            //uint r = 4294967295;
            //int r = -2147483648;
            //ulong r = 18446744073709551615;
            int r = -131;
            //float r = 123.7f;
            //double r = 123.7;
            //double d;
            //d = 0;
            //if (d > 39.0d)
            //{
            //    Console.WriteLine("Greater");
            //}
            //else
            //{
            //    Console.WriteLine("Less Than");
            //}
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
            Console.WriteLine("Byte->Decimal: " + (unchecked((decimal)r)).ToString());
            while (true)
            {
                //m.Update();
                //Console.WriteLine(Cosmos.Hardware.RTC.Hour.ToString() + ":" + Cosmos.Hardware.RTC.Minute.ToString() + ":" + Cosmos.Hardware.RTC.Second.ToString());
            }
        }
    }
}
