using System.IO;
using Cosmos.TestRunner;
using Cosmos.Debug.Kernel;

namespace Cosmos.Kernel.Tests.IO.System.IO
{
    public class BinaryWriterTest
    {
        /// <summary>
        /// Tests System.IO.BinaryWriter plugs.
        /// </summary>
        public static void Execute(Debugger mDebugger)
        {
            TestBinaryWriterOnMemoryStream(mDebugger);
        }

        private static void TestBinaryWriterOnMemoryStream(Debugger mDebugger)
        {
            mDebugger.Send("START TEST: Write on MemoryStream using BinaryWriter");
            mDebugger.Send("Writing data");

            using (var xMS = new MemoryStream())
            using (var xBW = new BinaryWriter(xMS))
            {
                xBW.Write(Kernel.xBytes);

                mDebugger.Send("Bytes written");
                xMS.Position = 0;
                long lengthO = xMS.Length;
                int lengthN = Kernel.xBytes.Length;
                Assert.IsTrue(lengthO == lengthN, "Failed to write bytes to MemoryStream");
            }

            mDebugger.Send("END TEST");
        }
    }
}
