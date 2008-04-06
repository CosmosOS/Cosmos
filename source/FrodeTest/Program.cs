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
            Cosmos.Kernel.Boot.Default();
            
            Cosmos.Kernel.Staging.DefaultStageQueue stages = new Cosmos.Kernel.Staging.DefaultStageQueue();
            stages.Enqueue(new Cosmos.Kernel.Staging.Stages.KernelStage());
            stages.Run();
            Security.User currentUser = Security.User.Authenticate("frode", "secret");
            Shell.Session currentSession =  Shell.Session.CreateSession(currentUser);
            currentSession.Run();

            //Test
            //Debug.SortedListSearcher.RunTest();
            //Test.SwitchTest.RunTest();
            //Test.RTL8139Test.RunTest();
            //Test.BinaryHelperTest.RunTest();
            //Test.TransmitStatusDescriptorTest.RunTest();
            //Test.PacketHeaderTest.RunTest();
            //Test.RAMBusTest.RunTest();
            //Test.BoolTest.RunTest();
            Test.InterfaceTest.RunTest();
            //Test.ExtensionMethodsTest.RunTest();
            //Test.NumberSystemTest.RunTest();
            Test.IPv4Test.RunTest();
            Test.UDPTest.RunTest();
            Test.StringTest.RunTest();
             
            //Done
            Console.WriteLine("Shutting down computer");
            while (true)
                ;
        }
    }
}