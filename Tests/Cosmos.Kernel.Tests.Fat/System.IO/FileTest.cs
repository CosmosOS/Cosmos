﻿using System;
using System.IO;
using Cosmos.TestRunner;
using Cosmos.Debug.Kernel;
using Cosmos.Common.Extensions;
using static Cosmos.Kernel.Tests.Fat.System.IO.HelperMethods;

namespace Cosmos.Kernel.Tests.Fat.System.IO
{
    class FileTest
    {
        /// <summary>
        /// Tests System.IO.File plugs.
        /// </summary>
        public static void Execute(Debugger mDebugger)
        {
            string xContents;
            // Moved this test here because if not the test can be executed only a time!
            mDebugger.Send("Write to file now");
            File.WriteAllText(@"0:\Kudzu.txt", "Hello Cosmos");
            mDebugger.Send("Text written");


            mDebugger.Send("File contents of Kudzu.txt: ");
            xContents = File.ReadAllText(@"0:\Kudzu.txt");
            mDebugger.Send("Contents retrieved");
            mDebugger.Send(xContents);
            Assert.IsTrue(xContents == "Hello Cosmos", "Contents of Kudzu.txt was read incorrectly!");
            mDebugger.Send("END TEST");
            mDebugger.Send("");

            //
            using (var xFS = new FileStream(@"0:\Kudzu.txt", FileMode.Open))
            {
                xFS.SetLength(5);
            }
            mDebugger.Send("File made smaller");
            xContents = File.ReadAllText(@"0:\Kudzu.txt");
            mDebugger.Send("Contents retrieved");
            mDebugger.Send(xContents);
            Assert.IsTrue(xContents == "Hello", "Contents of Kudzu.txt was read incorrectly!");
            mDebugger.Send("END TEST");
            mDebugger.Send("");

            //
            using (var xFS = new FileStream(@"0:\Kudzu.txt", FileMode.Create))
            {
                xFS.SetLength(5);
            }
            mDebugger.Send("File made smaller");
            xContents = File.ReadAllText(@"0:\Kudzu.txt");
            mDebugger.Send("Contents retrieved");
            mDebugger.Send(xContents);
            Assert.IsTrue(xContents == "Hello", "Contents of Kudzu.txt was read incorrectly!");
            mDebugger.Send("END TEST");
            mDebugger.Send("");

            //
            mDebugger.Send("Write to file now");
            File.WriteAllText(@"0:\Kudzu.txt", "Test FAT write.");
            mDebugger.Send("Text written");
            xContents = File.ReadAllText(@"0:\Kudzu.txt");
            mDebugger.Send("Contents retrieved after writing");
            mDebugger.Send(xContents);
            Assert.IsTrue(xContents == "Test FAT write.", "Contents of Kudzu.txt was written incorrectly!");
            mDebugger.Send("END TEST");
            mDebugger.Send("");

            //
            mDebugger.Send("START TEST: Create file:");
            // Attention! File.Create() returns a FileStream that should be Closed / Disposed on Windows trying to write to the file next gives "File in Use" exception!

            using (var xFile = File.Create(@"0:\test2.txt"))
            {
                Assert.IsTrue(xFile != null, "Failed to create a new file.");
                bool xFileExists = File.Exists(@"0:\test2.txt");
                Assert.IsTrue(xFileExists, "Failed to check existence of the new file.");
                mDebugger.Send("END TEST");
                mDebugger.Send("");
            }

            // Possible issue: writing to another file in the same directory, the data are mixed with the other files
            mDebugger.Send("Write to another file now");
            File.WriteAllText(@"0:\test2.txt", "123");
            mDebugger.Send("Text written");
            xContents = File.ReadAllText(@"0:\test2.txt");
            mDebugger.Send("Contents retrieved after writing");
            mDebugger.Send(xContents);
            Assert.IsTrue(xContents == "123", "Contents of test2.txt was written incorrectly!");
            mDebugger.Send("END TEST");
            mDebugger.Send("");

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

            // Generic methods on arrays don't work, see https://github.com/CosmosOS/Cosmos/issues/583
            //
            //string[] contents = { "One", "Two", "Three" };
            //File.WriteAllLines(@"0:\test3.txt", contents);
            //mDebugger.Send("Text written");
            //mDebugger.Send("Now reading with ReadAllLines()");
            //string[] readLines = File.ReadAllLines(@"0:\test3.txt");
            //mDebugger.Send("Contents retrieved after writing");
            //for (int i = 0; i < readLines.Length; i++)
            //{
            //    mDebugger.Send(readLines[i]);
            //}
            //Assert.IsTrue(StringArrayAreEquals(contents, readLines), "Contents of test3.txt was written incorrectly!");
#if false
                // TODO maybe the more correct test is to implement ReadAllLines and then check that two arrays are equals
                        var xContents = File.ReadAllText(@"0:\test3.txt");
                        mDebugger.Send("Contents retrieved after writing");
                        mDebugger.Send(xContents);
                        String expectedResult = String.Concat("One", Environment.NewLine, "Two", Environment.NewLine, "Three");
                        mDebugger.Send("expectedResult: " + expectedResult);
                        Assert.IsTrue(xContents == expectedResult, "Contents of test3.txt was written incorrectly!");
#endif
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
            }

            //mDebugger.Send("START TEST: Create a new directory with a file inside (File):");
            //var xDirectory2 = Directory.CreateDirectory(@"0:\testdir2");
            //Assert.IsTrue(xDirectory2 != null, "Failed to create a new directory.");
            //string WrittenText = "This a test";
            //File.WriteAllText(@"0:\testdir2\file.txt", WrittenText);
            //mDebugger.Send("Text written");
            //// now read it
            //xContents = File.ReadAllText(@"0:\testdir2\file.txt");
            //mDebugger.Send("Contents retrieved");
            //Assert.IsTrue(xContents == WrittenText, "Failed to read from file");

            //mDebugger.Send("START TEST: Append text to file:");
            //string appendedText = "Yet other text.";
            //File.AppendAllText(@"0:\Kudzu.txt", appendedText);
            //mDebugger.Send("Text appended");
            //xContents = File.ReadAllText(@"0:\Kudzu.txt");
            //mDebugger.Send("Contents retrieved after writing");
            //mDebugger.Send(xContents);
            //// XXX Use String.Concat() with Enviroment.NewLine this not Linux there are is '\n'!
            //Assert.IsTrue(xContents == "Test FAT write.\nYet other text.",
            //    "Contents of Kudzu.txt was appended incorrectly!");
            //mDebugger.Send("END TEST");
            //mDebugger.Send("");

            mDebugger.Send("START TEST: Delete a file:");
            File.Create(@"0:\test1.txt");
            Assert.IsTrue(File.Exists(@"0:\test1.txt"), "test1.txt wasn't created!");
            File.Delete(@"0:\test1.txt");
            Assert.IsFalse(File.Exists(@"0:\test1.txt"), "test1.txt wasn't deleted!");
            mDebugger.Send("END TEST");

            //mDebugger.Send("START TEST: Delete a directory with File.Delete:");
            //Simple test: create a directory, then try to delete it as a file.
            //Directory.CreateDirectory(@"0:\Dir1");

            //File.Delete(@"0:\Dir1");
            //Assert.IsTrue(Directory.Exists(@"0:\Dir1"), "Yeah, it's actually deleting the directory. That isn't right.");

            //mDebugger.Send("END TEST");
        }
    }
}
