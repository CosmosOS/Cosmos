using System;
using Sys = Cosmos.System;
using Cosmos.TestRunner;
using Cosmos.System.Graphics;
using System.Text;
using Cosmos.System.ExtendedASCII;
using Cosmos.System.ScanMaps;
using Cosmos.Core;
using System.Runtime.InteropServices;
using Cosmos.HAL;

namespace EnumTests
{
    public class Kernel : Sys.Kernel
    {
        protected override void BeforeRun()
        {
            Console.WriteLine("Cosmos booted successfully. Starting Tests");
        }

        protected override void Run()
        {
            try
            {
                TestVTableEnumStrings();

                TestController.Completed();
            }
            catch (Exception e)
            {
                mDebugger.Send("Exception occurred: " + e.Message);
                mDebugger.Send(e.Message);
                TestController.Failed();
            }
        }

        public void TestVTableEnumStrings() {
            Assert.AreEqual(VTablesImpl.GetEnumValueString((uint)VTablesImpl.GetType("TestEnum"), (uint)(byte)60), "CANCEL", "VTablesImpl.GetEnumValue does not work properly");
        }
    }

    public enum TestEnum : byte {
        OK = 3,
        CANCEL = 60,
        RESUME
    }
}
