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

namespace EnumTests {
    public class Kernel : Sys.Kernel {
        protected override void BeforeRun() {
            Console.WriteLine("Cosmos booted successfully. Starting Tests");
        }

        protected override void Run() {
            try {
                TestVTableEnumStrings();

                TestController.Completed();
            } catch (Exception e) {
                mDebugger.Send("Exception occurred: " + e.Message);
                mDebugger.Send(e.Message);
                TestController.Failed();
            }
        }

        public void TestVTableEnumStrings() {
            var testEnumVal = TestEnum.OK;
            uint x = (uint)testEnumVal; // random stuff to make TestEnum actually appear in the output

            Console.WriteLine(testEnumVal.ToString());

            uint typeId = (uint)VTablesImpl.GetType("TestEnum");

            Assert.AreNotEqual(typeId, unchecked((uint)-1), "TestEnum was resolved to not -1");
            Console.WriteLine((byte)VTablesImpl.GetEnumValueString(typeId, (uint)(byte)60)[12]);
            Assert.AreEqual("CANCEL", VTablesImpl.GetEnumValueString(typeId, (uint)(byte)60), "VTablesImpl.GetEnumValueString returned proper string");
        }
    }

    public enum TestEnum : byte {
        OK = 3,
        CANCEL = 60,
        RESUME
    }
}
