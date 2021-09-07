using System.IO;
using Cosmos.TestRunner;
using Cosmos.Debug.Kernel;

namespace Cosmos.Kernel.Tests.IO.System.IO
{
    public class BinaryReaderTest
    {
        /// <summary>
        /// Tests System.IO.BinaryReader plugs.
        /// </summary>
        public static void Execute(Debugger mDebugger)
        {
            TestBinaryReaderOnMemoryStream(mDebugger);
        }

        private static void TestBinaryReaderOnMemoryStream(Debugger mDebugger)
        {
            mDebugger.Send("START TEST: Read from MemoryStream using BinaryReader");
            mDebugger.Send("Writing data");

            byte[] xBuffer = new byte[16];

            using (var xMS = new MemoryStream(Kernel.xBytes))
            using (var xBR = new BinaryReader(xMS))
            {
                xBR.Read(xBuffer, 0, xBuffer.Length);

                mDebugger.Send("Data retrieved");

                foreach (byte b in xBuffer)
                {
                    mDebugger.Send("Byte: " + b.ToString());
                }

                Assert.IsTrue(HelperMethods.ByteArrayAreEquals(Kernel.xBytes, xBuffer), "Bytes changed during BinaryWriter and BinaryReader opeartions on MemoryStream");
            }
            mDebugger.Send("END TEST");

        }
    }
}
