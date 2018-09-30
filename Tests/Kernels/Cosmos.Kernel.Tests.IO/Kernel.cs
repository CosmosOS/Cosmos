using Cosmos.TestRunner;
using Sys = Cosmos.System;
using System;
using System.IO;

namespace Cosmos.Kernel.Tests.IO
{
    public class Kernel : Sys.Kernel
    {
        private bool ExecuteFileStreamTests = false;
        private Sys.FileSystem.VFS.VFSBase mVFS;

        private byte[] xBytes = new byte[16]
        {
            0x0, 0x1, 0x2, 0x3, 0x4, 0x5, 0x6, 0x7,
            0x8, 0x9, 0xA, 0xB, 0xC, 0xD, 0xE, 0xF
        };

        protected override void BeforeRun()
        {
            Console.WriteLine("Cosmos booted successfully. Starting IO tests.");

            if (ExecuteFileStreamTests)
            {
                mVFS = new Sys.FileSystem.CosmosVFS();
                Sys.FileSystem.VFS.VFSManager.RegisterVFS(mVFS);
            }
        }

        protected override void Run()
        {
            try
            {
                TestMemoryStreamByte();
                TestMemoryStreamReadBuffer();

                using (var xMS = new MemoryStream())
                {
                    TestBinaryWriterOnMemoryStream(xMS);
                }
                using (var nMS = new MemoryStream(xBytes))
                {
                    TestBinaryReaderOnMemoryStream(nMS);
                }

                if (ExecuteFileStreamTests)
                {
                //    TestMemoryStreamFromFileStream();
                }
                TestController.Completed();
            }
            catch(Exception e)
            {
                mDebugger.Send("Exception occurred: " + e.Message);

                TestController.Failed();
            }
        }

        #region System.IO.MemoryStream Tests

        private void TestMemoryStreamByte()
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

        private void TestMemoryStreamReadBuffer()
        {

            mDebugger.Send("START TEST: Create MemoryStream from byte array and read its bytes:");
            mDebugger.Send("Loading buffer");

            using (MemoryStream xMS = new MemoryStream(xBytes))
            {
                mDebugger.Send("Buffer loaded into memory");

                xMS.Position = 0;
                byte[] xBuffer = xMS.ToArray();

                foreach (byte b in xBuffer)
                {
                    mDebugger.Send("Byte: " + b.ToString());
                }

                Assert.IsTrue(ByteArrayAreEquals(xBytes, xBuffer), "Buffer was modified during its loading to memory.");
            }

            mDebugger.Send("END TEST");
        }

        private void TestMemoryStreamFromFileStream()
        {
            // For the following test to run we need .Net Core 2.1 or CopyTo has to be plugged
            //
            //using (var xFS = new FileStream(@"0:\test.txt", FileMode.OpenOrCreate))
            //{
            //    mDebugger.Send(@"Writing bytes to file 0:\test.txt");
            //    xFS.Write(xBytes, 0, xBytes.Length);
            //}

            //using (var xFS = new FileStream(@"0:\test.txt", FileMode.Open))
            //{
            //    mDebugger.Send(@"Reading bytes from file 0:\test.txt");
            //    byte[] xFileBytes = new byte[16];
            //    xFS.Read(xFileBytes, 0, xFileBytes.Length);

            //    Assert.IsTrue(ByteArrayAreEquals(xBytes, xFileBytes), "Bytes changed during FileStream write and read operations.");

            //    using (MemoryStream xMS = new MemoryStream())
            //    {
            //        xFS.CopyTo(xMS);
            //        byte[] xMemoryBytes = new byte[16];
            //        mDebugger.Send("Reading bytes from MemoryStream");

            //        xMS.Position = 0;
            //        xMS.Read(xMemoryBytes, 0, xMemoryBytes.Length);

            //        Assert.IsTrue(ByteArrayAreEquals(xBytes, xMemoryBytes), "Bytes changed during MemoryStream write and read operations.");
            //    }
            //}

            //mDebugger.Send("END TEST");
        }

        #endregion

        #region System.IO.BinaryWriter Tests

        private void TestBinaryWriterOnMemoryStream(MemoryStream xMS)
        {
            mDebugger.Send("START TEST: Write on MemoryStream using BinaryWriter");
            mDebugger.Send("Writing data");
            using (var xBW = new BinaryWriter(xMS))
            {
                xBW.Write(xBytes);

                mDebugger.Send("Bytes written");
                xMS.Position = 0;
                long lengthO = xMS.Length;
                int lengthN = xBytes.Length;
                Assert.IsTrue(lengthO == lengthN, "Failed to write bytes to MemoryStream");
            }
            mDebugger.Send("END TEST");

        }

        #endregion

        #region System.IO.BinaryReader Tests

        private void TestBinaryReaderOnMemoryStream(MemoryStream xMS)
        {

            mDebugger.Send("START TEST: Read from MemoryStream using BinaryReader");
            mDebugger.Send("Writing data");

            byte[] xBuffer = new byte[16];

            using (var xBR = new BinaryReader(xMS))
            {
                xBR.Read(xBuffer, 0, xBuffer.Length);

                mDebugger.Send("Data retrieved");

                foreach (byte b in xBuffer)
                {
                    mDebugger.Send("Byte: " + b.ToString());
                }

                Assert.IsTrue(ByteArrayAreEquals(xBytes, xBuffer), "Bytes changed during BinaryWriter and BinaryReader opeartions on MemoryStream");
            }
            mDebugger.Send("END TEST");

        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Utility method to test Byte[] equality.
        /// </summary>
        /// <param name="a1">Byte array.</param>
        /// <param name="a2">Byte array.</param>
        /// <returns>True if the elements in the arrays are equal otherwise false.</returns>
        private bool ByteArrayAreEquals(byte[] a1, byte[] a2)
        {
            if (ReferenceEquals(a1, a2))
            {
                mDebugger.Send("a1 and a2 are the same Object");
                return true;
            }

            if (a1 == null || a2 == null)
            {
                mDebugger.Send("a1 or a2 is null so are different");
                return false;
            }

            if (a1.Length != a2.Length)
            {
                mDebugger.Send("a1.Length != a2.Length so are different");
                return false;
            }

            for (int i = 0; i < a1.Length; i++)
            {
                if (a1[i] != a2[i])
                {
                    mDebugger.Send("In position " + i + " a byte is different(" + a1[i].ToString()  + " vs " + a2[i].ToString() + ")");
                    return false;
                }
            }

            return true;
        }

        #endregion
    }
}
