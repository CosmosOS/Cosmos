using System.IO;
using Cosmos.TestRunner;
using Cosmos.Debug.Kernel;

namespace Cosmos.Kernel.Tests.IO.System.IO
{
    public class MemoryStreamTest
    {
        /// <summary>
        /// Tests System.IO.MemoryStream plugs.
        /// </summary>
        public static void Execute(Debugger mDebugger)
        {
            TestMemoryStreamByte(mDebugger);
            TestMemoryStreamReadBuffer(mDebugger);

            /* It needs Stream.CopyTo() to be plugged */
            //if (ExecuteFileStreamTests)
            //{
                //TestMemoryStreamFromFileStream();
            //}
        }

        private static void TestMemoryStreamByte(Debugger mDebugger)
        {
            mDebugger.Send("START TEST: MemoryStream:");
            mDebugger.Send("Start writing");

            using (var xMS = new MemoryStream())
            {
                byte bWrite = 0x30;
                xMS.WriteByte(bWrite);

                mDebugger.Send("Byte written");

                xMS.Position = 0;
                byte bRead = (byte)xMS.ReadByte();

                mDebugger.Send("Written byte: " + bWrite.ToString() + "    Read byte: " + bRead.ToString());

                Assert.IsTrue(bWrite == bRead, "Failed to write a byte and read it from memory.");
            }
            mDebugger.Send("END TEST");
        }

        private static void TestMemoryStreamReadBuffer(Debugger mDebugger)
        {

            mDebugger.Send("START TEST: Create MemoryStream from byte array and read its bytes:");
            mDebugger.Send("Loading buffer");

            using (MemoryStream xMS = new MemoryStream(Kernel.xBytes))
            {
                mDebugger.Send("Buffer loaded into memory");

                xMS.Position = 0;
                byte[] xBuffer = xMS.ToArray();

                foreach (byte b in xBuffer)
                {
                    mDebugger.Send("Byte: " + b.ToString());
                }

                Assert.IsTrue(HelperMethods.ByteArrayAreEquals(Kernel.xBytes, xBuffer), "Buffer was modified during its loading to memory.");
            }

            mDebugger.Send("END TEST");
        }

        private static void TestMemoryStreamFromFileStream(Debugger mDebugger)
        {
            // For the following test to run we need .Net Core 2.1 or CopyTo has to be plugged
            //
            //using (var xFS = new FileStream(@"0:\test.txt", FileMode.OpenOrCreate))
            //{
            //    mDebugger.Send(@"Writing bytes to file 0:\test.txt");
            //    xFS.Write(Kernel.xBytes, 0, Kernel.xBytes.Length);
            //}

            //using (var xFS = new FileStream(@"0:\test.txt", FileMode.Open))
            //{
            //    mDebugger.Send(@"Reading bytes from file 0:\test.txt");
            //    byte[] xFileBytes = new byte[16];
            //    xFS.Read(xFileBytes, 0, xFileBytes.Length);

            //    Assert.IsTrue(ByteArrayAreEquals(Kernel.xBytes, xFileBytes), "Bytes changed during FileStream write and read operations.");

            //    using (MemoryStream xMS = new MemoryStream())
            //    {
            //        xFS.CopyTo(xMS);
            //        byte[] xMemoryBytes = new byte[16];
            //        mDebugger.Send("Reading bytes from MemoryStream");

            //        xMS.Position = 0;
            //        xMS.Read(xMemoryBytes, 0, xMemoryBytes.Length);

            //        Assert.IsTrue(ByteArrayAreEquals(Kernel.xBytes, xMemoryBytes), "Bytes changed during MemoryStream write and read operations.");
            //    }
            //}

            //mDebugger.Send("END TEST");
        }
    }
}
