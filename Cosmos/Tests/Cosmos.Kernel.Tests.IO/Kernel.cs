using System;
using System.IO;
using System.Text;
using Cosmos.Common.Extensions;
using Cosmos.TestRunner;
using Sys = Cosmos.System;
using Cosmos.System.FileSystem;
using Cosmos.System.FileSystem.VFS;

namespace Cosmos.Kernel.Tests.IO
{
    public class Kernel : Sys.Kernel
    {
        private bool ExecuteFileStreamTests = false;
        private VFSBase mVFS;

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
                mVFS = new CosmosVFS();
                VFSManager.RegisterVFS(mVFS);
            }
        }

        /*Error Stack Trace:
         *
         *Error: Exception: System.Exception: Error compiling method 'SystemVoidSystemIOMemoryStreamDisposeSystemBoolean': System.NullReferenceException: Object reference not set to an instance of an object.
         *at Cosmos.IL2CPU.X86.IL.Leave.Execute(MethodInfo aMethod, ILOpCode aOpCode) in Cosmos\source\Cosmos.IL2CPU\IL\Leave.cs:line 17
         *at Cosmos.IL2CPU.AppAssembler.EmitInstructions(MethodInfo aMethod, List`1 aCurrentGroup, Boolean & amp; emitINT3) in Cosmos\source\Cosmos.IL2CPU\AppAssembler.cs:line 667
         *at Cosmos.IL2CPU.AppAssembler.ProcessMethod(MethodInfo aMethod, List`1 aOpCodes) in Cosmos\source\Cosmos.IL2CPU\AppAssembler.cs:line 533-- - >; System.NullReferenceException: Object reference not set to an instance of an object.
         *at Cosmos.IL2CPU.X86.IL.Leave.Execute(MethodInfo aMethod, ILOpCode aOpCode) in Cosmos\source\Cosmos.IL2CPU\IL\Leave.cs:line 17
         *at Cosmos.IL2CPU.AppAssembler.EmitInstructions(MethodInfo aMethod, List`1 aCurrentGroup, Boolean & amp; emitINT3) in Cosmos\source\Cosmos.IL2CPU\AppAssembler.cs:line 667
         *at Cosmos.IL2CPU.AppAssembler.ProcessMethod(MethodInfo aMethod, List`1 aOpCodes) in Cosmos\source\Cosmos.IL2CPU\AppAssembler.cs:line 533
         *--- End of inner exception stack trace ---
         *at Cosmos.IL2CPU.AppAssembler.ProcessMethod(MethodInfo aMethod, List`1 aOpCodes) in Cosmos\source\Cosmos.IL2CPU\AppAssembler.cs:line 540
         *at Cosmos.IL2CPU.ILScanner.Assemble() in Cosmos\source\Cosmos.IL2CPU\ILScanner.cs:line 946
         *at Cosmos.IL2CPU.ILScanner.Execute(MethodBase aStartMethod) in Cosmos\source\Cosmos.IL2CPU\ILScanner.cs:line 247
         *at Cosmos.IL2CPU.CompilerEngine.Execute() in Cosmos\source\Cosmos.IL2CPU\CompilerEngine.cs:line 252
         *
         */

        protected override void Run()
        {
            try
            {
                mDebugger.Send("Run");

                TestMemoryStreamByte();
                //TestMemoryStreamReadBuffer();

                /*using (var xMS = new MemoryStream())
                {
                    TestBinaryWriterOnMemoryStream(xMS);
                    TestBinaryReaderOnMemoryStream(xMS);
                }*/

                if (ExecuteFileStreamTests)
                {
                    //TestMemoryStreamFromFileStream();
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
            /*
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
            */
        }

        private void TestMemoryStreamReadBuffer()
        {
            /*
            mDebugger.Send("START TEST: Create MemoryStream from byte array and read its bytes:");
            mDebugger.Send("Loading buffer");

            using (MemoryStream xMS = new MemoryStream(xBytes))
            {
                mDebugger.Send("Buffer loaded into memory");

                xMS.Position = 0;
                byte[] xBuffer = xMS.GetBuffer();

                mDebugger.Send("Buffer retrieved from memory");

                foreach (byte b in xBuffer)
                {
                    mDebugger.Send("Byte:");
                    mDebugger.Send(b.ToString());
                }

                Assert.IsTrue(ByteArrayAreEquals(xBytes, xBuffer), "Buffer was modified during its loading to memory.");
            }

            mDebugger.Send("END TEST");
            */
        }

        private void TestMemoryStreamFromFileStream()
        {
            /*
            mDebugger.Send("START TEST: Create FileStream from byte array and copy FileStream to MemoryStream:");
            mDebugger.Send(@"Creating file 0:\test.txt");

            using (FileStream xFS = new FileStream(@"0:\test.txt", FileMode.Create))
            {
                mDebugger.Send(@"Writing bytes to file 0:\test.txt");

                xFS.Write(xBytes, 0, xBytes.Length);
                byte[] xFileBytes = new byte[16];

                mDebugger.Send(@"Reading bytes from file 0:\test.txt");

                xFS.Read(xFileBytes, 0, xFileBytes.Length);

                Assert.IsTrue(ByteArrayAreEquals(xBytes, xFileBytes), "Bytes changed during FileStream write and read operations.");

                using (MemoryStream xMS = new MemoryStream())
                {
                    mDebugger.Send("MemoryStream created");

                    xFS.CopyTo(xMS);
                    byte[] xMemoryBytes = new byte[16];

                    mDebugger.Send("Reading bytes from MemoryStream");

                    xMS.Position = 0;
                    xMS.Read(xMemoryBytes, 0, xMemoryBytes.Length);

                    Assert.IsTrue(ByteArrayAreEquals(xBytes, xMemoryBytes), "Bytes changed during MemoryStream write and read operations.");
                }
            }

            mDebugger.Send("END TEST");
            */
        }

        #endregion

        #region System.IO.BinaryWriter Tests

        private void TestBinaryWriterOnMemoryStream(MemoryStream xMS)
        {
            /*
            mDebugger.Send("START TEST: Write on MemoryStream using BinaryWriter");
            mDebugger.Send("Writing data");

            using (var xBW = new BinaryWriter(xMS))
            {
                xBW.Write(xBytes);

                mDebugger.Send("Bytes written");
            }

            Assert.IsTrue(xMS.Length == xBytes.Length, "Failed to write bytes to MemoryStream");

            mDebugger.Send("END TEST");
            */
        }

        #endregion

        #region System.IO.BinaryReader Tests

        private void TestBinaryReaderOnMemoryStream(MemoryStream xMS)
        {
            /*
            mDebugger.Send("START TEST: Read from MemoryStream using BinaryReader");
            mDebugger.Send("Writing data");

            byte[] xBuffer = new byte[16];

            using (var xBR = new BinaryReader(xMS))
            {
                xBR.Read(xBuffer, 0, xBuffer.Length);

                mDebugger.Send("Data retrieved");

                foreach (byte b in xBuffer)
                {
                    mDebugger.Send("Byte:");
                    mDebugger.Send(b.ToString());
                }

                Assert.IsTrue(ByteArrayAreEquals(xBytes, xBuffer), "Bytes changed during BinaryWriter and BinaryReader opeartions on MemoryStream");
            }
            mDebugger.Send("END TEST");
            */
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
                    mDebugger.Send("In position " + i + " a byte is different");
                    return false;
                }
            }

            return true;
        }

        #endregion
    }
}
