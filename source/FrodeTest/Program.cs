using System;
using Cosmos.Build.Windows;

namespace FrodeTest
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
                //while (true)
                //Console.Beep(2000, 2);
                Console.Beep();
                    //Cosmos.Hardware.PIT.PlaySound(3000);
                //Console.Beep();
                //Shell.Session.Run();
                //Console.WriteLine(b);

                //Testing \\-string bug
                //Console.Write('1');
                //PrintBackslash();
                //Console.Write('2');
                //WriteChar(92, 10, 20, 1, 1);
                //Console.Write('3');
                ////Cosmos.Hardware.TextScreen.WriteChar('\\');
                //Console.Write('4');
                ////Console.Write('\\');
                //Console.Write('5');
                ////Console.Write("\\");
                //Console.WriteLine(@"\");

                //TEST FRAMEWORK
                //Console.WriteLine("---- RUNNING PREDEFINED TESTS ----");
                //Test.ConsoleTest.RunTest();
                //Test.StringTest.RunTest();
                //Test.IPv4AddressTest.RunTest();
                //Test.BasicTest.RunTest();
                //Test.SwitchTest.RunTest();
                //Console.ReadLine();
                //Test.BoolTest.RunTest();
                //Test.InterfaceTest.RunTest();
                //Test.ExtensionMethodsTest.RunTest();
                //Test.BinaryHelperTest.RunTest();
                //Test.TransmitStatusDescriptorTest.RunTest();
                //Test.PacketHeaderTest.RunTest();
                //Test.RAMBusTest.RunTest();
                //Test.NumberSystemTest.RunTest();
                //Test.IPv4Test.RunTest();
                //Test.UDPTest.RunTest();
                //Test.MACAddressTest.RunTest();
                //Test.LinqTest.RunTest();
                //Test.ExceptionTest.RunTest();
                //Test.FilesystemEntryTest.RunTest();
                //Test.VirtualFileSystemTest.RunTest();
                //Test.Ext2Test.RunTest();
                //Test.DirectoryTest.RunTest();
                //Test.FileTest.RunTest();
                //Test.DirectoryInfoTest.RunTest();
                //Test.FileInfoTest.RunTest();

                //Tests ready for Matthijs to fix ;)
                //Test.RTL8139Test.RunTest();
                //Test.ExceptionTest.RunTest();
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