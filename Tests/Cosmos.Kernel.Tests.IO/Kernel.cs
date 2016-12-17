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
        private bool ExecuteFileStreamTests = true;
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

                TestFile(); //TODO: Move back to FAT test kernel, when FAT is faster

                TestMemoryStreamByte();
                TestMemoryStreamReadBuffer();

                /*using (var xMS = new MemoryStream())
                {
                    TestBinaryWriterOnMemoryStream(xMS);
                    TestBinaryReaderOnMemoryStream(xMS);
                }*/

                if (ExecuteFileStreamTests)
                {
                    TestMemoryStreamFromFileStream();
                }

                TestController.Completed();
            }
            catch(Exception e)
            {
                mDebugger.Send("Exception occurred: " + e.Message);

                TestController.Failed();
            }
        }

        #region FAT File Tests

        private void TestFile()
        {
            // Now we write in test3.txt using WriteAllLines()
            mDebugger.Send("START TEST: WriteAllLines:");
            using (var xFile = File.Create(@"0:\test3.txt"))
            {
                Assert.IsTrue(xFile != null, "Failed to create a new file.");
                bool xFileExists = File.Exists(@"0:\test3.txt");
                Assert.IsTrue(xFileExists, "Failed to check existence of the new file.");
                mDebugger.Send("END TEST");
                mDebugger.Send("");
            }


            string[] contents = { "One", "Two", "Three" };
            File.WriteAllLines(@"0:\test3.txt", contents);
            mDebugger.Send("Text written");
            mDebugger.Send("Now reading with ReadAllLines()");
            string[] readLines = File.ReadAllLines(@"0:\test3.txt");
            mDebugger.Send("Contents retrieved after writing");
            for (int i = 0; i < readLines.Length; i++)
            {
                mDebugger.Send(readLines[i]);
            }
            Assert.IsTrue(StringArrayAreEquals(contents, readLines), "Contents of test3.txt was written incorrectly!");

            // TODO maybe the more correct test is to implement ReadAllLines and then check that two arrays are equals
            var xContents = File.ReadAllText(@"0:\test3.txt");
            mDebugger.Send("Contents retrieved after writing");
            mDebugger.Send(xContents);
            string expectedResult = string.Concat("One", Environment.NewLine, "Two", Environment.NewLine, "Three", Environment.NewLine);
            mDebugger.Send("expectedResult: " + expectedResult);
            Assert.IsTrue(xContents == expectedResult, "Contents of test3.txt was written incorrectly!");

            mDebugger.Send("END TEST");
            mDebugger.Send("");

            //
            mDebugger.Send("START TEST: Write binary data to file now:");
            using (var xFile = File.Create(@"0:\test.dat"))
            {
                Assert.IsTrue(xFile != null, "Failed to create a new file.");
            }
            byte[] dataWritten = new byte[] { 0x01, 0x02, 0x03 };
            File.WriteAllBytes(@"0:\test.dat", dataWritten);
            mDebugger.Send("Text written");
            byte[] dataRead = File.ReadAllBytes(@"0:\test.dat");

            Assert.IsTrue(ByteArrayAreEquals(dataWritten, dataRead), "Failed to write binary data to a file.");
            mDebugger.Send("END TEST");
            mDebugger.Send("");

            mDebugger.Send("START TEST: Create a new directory with a file inside (filestream):");
            var xDirectory = Directory.CreateDirectory(@"0:\testdir");
            Assert.IsTrue(xDirectory != null, "Failed to create a new directory.");
            using (var xFile2 = File.Create(@"0:\testdir\file.txt"))
            {
                string wText = "This a test";
                byte[] xWriteBuff = wText.GetUtf8Bytes(0, (uint)wText.Length);
                xFile2.Write(xWriteBuff, 0, xWriteBuff.Length);
                mDebugger.Send("---- Data written");
                xFile2.Position = 0;
                byte[] xReadBuff = new byte[xWriteBuff.Length];
                xFile2.Read(xReadBuff, 0, xWriteBuff.Length);
                mDebugger.Send("xWriteBuff=" + xWriteBuff.GetUtf8String(0, (uint)xWriteBuff.Length));
                mDebugger.Send("xReadBuff =" + xReadBuff.GetUtf8String(0, (uint)xReadBuff.Length));
                string xWriteBuffAsString = xWriteBuff.GetUtf8String(0, (uint)xWriteBuff.Length);
                string xReadBuffAsString = xReadBuff.GetUtf8String(0, (uint)xReadBuff.Length);
                mDebugger.Send("xWriteBuffAsString=" + xWriteBuffAsString);
                mDebugger.Send("xReadBuffAsString =" + xReadBuffAsString);
                Assert.IsTrue(xWriteBuffAsString == xReadBuffAsString, "Failed to write and read file");
                mDebugger.Send("END TEST");
                mDebugger.Send("");
            }

            mDebugger.Send("START TEST: Create a new directory with a file inside (File):");
            var xDirectory2 = Directory.CreateDirectory(@"0:\testdir2");
            Assert.IsTrue(xDirectory2 != null, "Failed to create a new directory.");
            string WrittenText = "This a test";
            File.WriteAllText(@"0:\testdir2\file.txt", WrittenText);
            mDebugger.Send("Text written");
            xContents = File.ReadAllText(@"0:\testdir2\file.txt");
            mDebugger.Send("Contents retrieved");
            Assert.IsTrue(xContents == WrittenText, "Failed to read from file");
            mDebugger.Send("END TEST");
            mDebugger.Send("");

            mDebugger.Send("START TEST: Append text to file:");
            string appendedText = Environment.NewLine + "Yet other text.";
            File.AppendAllText(@"0:\Kudzu.txt", appendedText);
            mDebugger.Send("Text appended");
            xContents = File.ReadAllText(@"0:\Kudzu.txt");
            mDebugger.Send("Contents retrieved after writing");
            mDebugger.Send(xContents);
            //Assert.IsTrue(xContents == "Test FAT write." + Environment.NewLine + "Yet other text.", "Contents of Kudzu.txt was appended incorrectly!");
            Assert.IsTrue(xContents == "Hello Cosmos" + Environment.NewLine + "Yet other text.", "Contents of Kudzu.txt was appended incorrectly!");
            mDebugger.Send("END TEST");
            mDebugger.Send("");

            mDebugger.Send("START TEST: Delete a file:");
            File.Create(@"0:\test1.txt");
            Assert.IsTrue(File.Exists(@"0:\test1.txt"), "test1.txt wasn't created!");
            File.Delete(@"0:\test1.txt");
            Assert.IsFalse(File.Exists(@"0:\test1.txt"), "test1.txt wasn't deleted!");
            mDebugger.Send("END TEST");
            mDebugger.Send("");

            mDebugger.Send("START TEST: Delete a directory with File.Delete:");
            Directory.CreateDirectory(@"0:\Dir1");

            try
            {
                File.Delete(@"0:\Dir1");
            }
            catch (Exception e)
            {
                Assert.IsTrue(Directory.Exists(@"0:\Dir1"), "The directory was deleted by File.Delete.");
            }

            mDebugger.Send("END TEST");
            mDebugger.Send("");

            mDebugger.Send("START TEST: Create a directory and a file in that directory and write to that file:");
            Directory.CreateDirectory(@"0:\testdir");
            mDebugger.Send("Directory created");
            File.Create(@"0:\testdir\testfile.txt");
            mDebugger.Send("File created");
            File.WriteAllText(@"0:\testdir\testfile.txt", "Hello Cosmos!");
            mDebugger.Send("Text written");
            Assert.IsTrue(File.ReadAllText(@"0:\testdir\testfile.txt") == "Hello Cosmos!", "File was not written correctly");
            mDebugger.Send("END TEST");
            mDebugger.Send("");

            mDebugger.Send("START TEST: Create a file with a Long Filename");
            File.Create(@"0:\testdir\LongFilename.txt");
            mDebugger.Send("File created");
            File.WriteAllText(@"0:\testdir\LongFilename.txt", "Hello Cosmos!");
            mDebugger.Send("Text written");
            Assert.IsTrue(File.ReadAllText(@"0:\testdir\LongFilename.txt") == "Hello Cosmos!", "Contents weren't correctly written");
            mDebugger.Send("END TEST");
            mDebugger.Send("");
        }

        #endregion

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

        /// <summary>
        /// Utility method to test string[] equality.
        /// </summary>
        /// <param name="a1">String array.</param>
        /// <param name="a2">String array.</param>
        /// <returns>True if the elements in the arrays are equal otherwise false.</returns>
        private bool StringArrayAreEquals(string[] a1, string[] a2)
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
                    mDebugger.Send("In position " + i + " a String is different");
                    return false;
                }
            }

            return true;
        }

        #endregion
    }
}
