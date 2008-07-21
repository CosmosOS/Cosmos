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
            Cosmos.Sys.Boot.Default();

            Console.WriteLine("*** COSMOS Operating System - Frode's Test Suite ***");
            //Console.WriteLine("CPU: " + Cosmos.Kernel.CPU.CPUVendor);

            Shell.Session.Run();

            //Test
            Console.WriteLine("---- RUNNING PREDEFINED TESTS ----");
            //Test.StringTest.RunTest();
            //Test.IPv4AddressTest.RunTest();
            //Test.BasicTest.RunTest();
            //Test.SwitchTest.RunTest();
            
            //Test.BoolTest.RunTest();
            //Test.InterfaceTest.RunTest();
            //Test.ExtensionMethodsTest.RunTest();
            //Test.BinaryHelperTest.RunTest();
            //Test.TransmitStatusDescriptorTest.RunTest();
            //Test.PacketHeaderTest.RunTest();
            //Test.RAMBusTest.RunTest();
            //Test.RTL8139Test.RunTest();
            //Test.NumberSystemTest.RunTest();
            //Test.IPv4Test.RunTest();
            //Test.UDPTest.RunTest();
            //Test.MACAddressTest.RunTest();
            //Test.LinqTest.RunTest();
             
            //Done
            Console.WriteLine("Shutting down computer");
            Cosmos.Sys.Deboot.ShutDown();            
        }
    }
}