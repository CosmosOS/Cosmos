using System.IO;
using Cosmos.TestRunner;
using Cosmos.Debug.Kernel;
using Cosmos.Common.Extensions;
using static Cosmos.Kernel.Tests.Fat.System.IO.HelperMethods;
using System;
using System.Linq;
using System.Collections.Generic;

namespace Cosmos.Kernel.Tests.Fat.System.IO
{
    class FileStreamTest
    {
        /// <summary>
        /// Tests System.IO.FileStream plugs.
        /// </summary>
        public static void Execute(Debugger mDebugger)
        {
            mDebugger.Send("START TEST: Filestream:");

            byte[] xWriteBuff = new byte[] { 0, 1, 2 };
            byte[] xAppendBuff = new byte[] { 3, 4 };
            byte[] xSumOfAbove = new byte[] { 0, 1, 2, 3, 4 };

            mDebugger.Send("New FileStream with FileMode.Create (file not existing)");
            using (var xFS = new FileStream(@"0:\fsTest", FileMode.Create))
            {
                // The File should exist now
                Assert.IsTrue(File.Exists(@"0:\fsTest") == true, "Filestream with FileMode.Create failed: File not created");

                mDebugger.Send("Start writing");

                xFS.Write(xWriteBuff, 0, xWriteBuff.Length);
                mDebugger.Send("---- Data written");
                xFS.Position = 0;
                byte[] xReadBuff = new byte[xWriteBuff.Length];
                xFS.Read(xReadBuff, 0, xWriteBuff.Length);

                Assert.IsTrue(ByteArrayAreEquals(xWriteBuff, xReadBuff), "Failed to write and read file");
                mDebugger.Send("END TEST");
                mDebugger.Send("");
            }

            mDebugger.Send("New FileStream with FileMode.Append (file existing)");
            using (var xFS = new FileStream(@"0:\fsTest", FileMode.Append))
            {
                // The file position should not have been resetted by 'FileMode.Append'
                Assert.IsTrue(xFS.Position == xWriteBuff.Length, "Filestream with FileMode.Append failed: file position wrong");

                mDebugger.Send("Start writing");
 
                xFS.Write(xAppendBuff, 0, xAppendBuff.Length);
                mDebugger.Send("---- Data written");
                xFS.Position = 0;
                mDebugger.Send("Start reading");
                byte[] xReadBuff = new byte[xAppendBuff.Length + 3];
                xFS.Read(xReadBuff, 0, xReadBuff.Length);

                Assert.IsTrue(ByteArrayAreEquals(xSumOfAbove, xReadBuff), "Failed to append and read file");
                mDebugger.Send("END TEST");
                mDebugger.Send("");
            }

            mDebugger.Send("New FileStream with FileMode.Open (file existing)");
            using (var xFS = new FileStream(@"0:\fsTest", FileMode.Open))
            {
                // We read its content again expecting to real 'xSumOfAbove' again

                mDebugger.Send("Start reading");
                byte[] xReadBuff = new byte[xAppendBuff.Length + 3];
                xFS.Read(xReadBuff, 0, xReadBuff.Length);

                Assert.IsTrue(ByteArrayAreEquals(xSumOfAbove, xReadBuff), "Failed to read file");
                mDebugger.Send("END TEST");
                mDebugger.Send("");
            }

            mDebugger.Send("New FileStream with FileMode.OpenOrCreate (file existing)");
            using (var xFS = new FileStream(@"0:\fsTest", FileMode.OpenOrCreate))
            {
                // We read its content again expecting to real 'xSumOfAbove' again

                mDebugger.Send("Start reading");
                byte[] xReadBuff = new byte[xAppendBuff.Length + 3];
                xFS.Read(xReadBuff, 0, xReadBuff.Length);

                Assert.IsTrue(ByteArrayAreEquals(xSumOfAbove, xReadBuff), "Failed to read file");
                mDebugger.Send("END TEST");
                mDebugger.Send("");
            }

            mDebugger.Send("New FileStream with FileMode.Create (file existing)");
            using (var xFS = new FileStream(@"0:\fsTest", FileMode.Create))
            {
                // The file should have been truncated
                Assert.IsTrue(xFS.Length == 0, "File.Create (truncate) does not work: file not empty");
            }

            /*
             * This could be linked to https://github.com/CosmosOS/Cosmos/issues/760
             * We Open again the file and this time we write less content, it is expected
             * that no traces of the previous data should exist
             */
            mDebugger.Send("New FileStream with FileMode.Open (write less after truncation)");
            xWriteBuff = new byte[] { 7, 8 };
            using (var xFS = new FileStream(@"0:\fsTest", FileMode.Open))
            {
                mDebugger.Send("Start writing");

                xFS.Write(xWriteBuff, 0, xWriteBuff.Length);
                mDebugger.Send("---- Data written");
                xFS.Position = 0;
                byte[] xReadBuff = new byte[xWriteBuff.Length];
                xFS.Read(xReadBuff, 0, xWriteBuff.Length);
                Assert.IsTrue(ByteArrayAreEquals(xWriteBuff, xReadBuff), "Failed to write and read file");
                mDebugger.Send("END TEST");
                mDebugger.Send("");

            }

            mDebugger.Send("New FileStream with FileMode.OpenOrCreate (file not existing)");
            using (var xFS = new FileStream(@"0:\fsTest2", FileMode.OpenOrCreate))
            {
                // It should Create a new File, so the file exists?
                Assert.IsTrue(File.Exists(@"0:\fsTest2") == true, "Filestream with FileMode.OpenOrCreate failed: File not created");

                // The file is "new" that is empty?
                Assert.IsTrue(xFS.Length == 0, "Filestream with FileMode.OpenOrCreate failed: File not empty");
            }

            /*
             * The exception is correctly thrown but it is not caught! Bug of TestRunner?
             */
            //bool xGotExpectedException = false;

            //mDebugger.Send("New FileStream with FileMode.Open (file not existing)");
            //try
            //{
            //    using (var xFS = new FileStream(@"0:\fsTest3", FileMode.Open))
            //    {
            //        mDebugger.Send("Got no exceptions (bad!)");
            //        xGotExpectedException = false;
            //    }
            //}
            //catch // other Exception
            //{
            //    mDebugger.Send("Got unexpected exception (bad!)");
            //    xGotExpectedException = true;
            //}

            //Assert.IsTrue(xGotExpectedException == true, "FileStream with FileMode.Open of not existing file does not throw FileNotFoundException");
        }
    }
}
