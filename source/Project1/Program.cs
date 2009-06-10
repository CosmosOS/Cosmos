using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cosmos.Compiler.Builder;

namespace Project1
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

        // Main entry point of the kernel
        //This is the playground for Frode "Scalpel" Lillerud.
        public static void Init()
        {
            //var xBoot = new Cosmos.Sys.Boot();
            //xBoot.Execute();

            new Cosmos.Sys.Boot().Execute();

            Console.WriteLine("*** COSMOS Operating System - Frode's Test Suite ***");
            try
            {
              


            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("General error in FrodeTest: " + ex.Message);
            }
             
            //Done
            Console.WriteLine();
            Cosmos.Sys.Deboot.ShutDown();            
        }

        public static unsafe void PrintBackslash()
        {
            byte* xTestByte = (byte*)(0xB8011 + 160);
            *xTestByte = 65;
        }

        public static unsafe void WriteChar(byte aChar, byte forecolor, byte backcolor, int x, int y)
        {
            short attrib = (short)((backcolor << 4) | (forecolor & 0x0F));
            short* where;
            where = (short*)0xB8000 + (y * 80 + x);
            *where = (short)(aChar | (attrib << 8));
        }
    }
    }
}
