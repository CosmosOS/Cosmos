using System;
using System.IO;
using Cosmos.TestRunner;
using Cosmos.Debug.Kernel;

namespace Cosmos.Kernel.Tests.Fat.System.IO
{
    class DirectoryTest
    {
        /// <summary>
        /// Tests System.IO.Directory plugs.
        /// </summary>
        public static void Execute(Debugger mDebugger)
        {

            mDebugger.Send("START TEST: Delete a directory:");
            Directory.CreateDirectory(@"0:\TestDir1");
            Assert.IsTrue(Directory.Exists(@"0:\TestDir1"), "TestDir1 wasn't created!");
            Directory.Delete(@"0:\TestDir1");
            Assert.IsFalse(Directory.Exists(@"0:\TestDir1"), "TestDir1 wasn't deleted!");
            mDebugger.Send("END TEST");

            mDebugger.Send("");

            mDebugger.Send("START TEST: Get parent:");
            var xParent = Directory.GetParent(@"0:\test");
            Assert.IsTrue(xParent != null, "Failed to get directory parent.");
            mDebugger.Send("END TEST");

            mDebugger.Send("");

            //
            mDebugger.Send("START TEST: Get parent:");
            xParent = Directory.GetParent(@"0:\test\");
            Assert.IsTrue(xParent != null, "Failed to get directory parent.");
            mDebugger.Send("END TEST");

            mDebugger.Send("");

            //
            mDebugger.Send("START TEST: Get files:");
            var xFiles = Directory.GetFiles(@"0:\");
            mDebugger.Send("Found " + xFiles.Length + " files.");
            if (xFiles.Length > 0)
            {
                mDebugger.Send("-- File list");
                for (int i = 0; i < xFiles.Length; i++)
                {
                    mDebugger.Send("File: " + xFiles[i]);
                }
            }
            Assert.IsTrue(xFiles.Length > 0, "Failed to get files from the directory.");
            mDebugger.Send("END TEST");

            mDebugger.Send("");

            //
            mDebugger.Send("START TEST: Get directories:");
            var xDirectories = Directory.GetDirectories(@"0:\");
            mDebugger.Send("Found " + xDirectories.Length + " directories.");
            if (xDirectories.Length > 0)
            {
                mDebugger.Send("-- Directory list");
                for (int i = 0; i < xDirectories.Length; i++)
                {
                    mDebugger.Send("Directory: " + xDirectories[i]);
                }
            }
            Assert.IsTrue(xDirectories.Length > 0, "Failed to get directories from the directory.");
            mDebugger.Send("END TEST");

            mDebugger.Send("");

            //
            mDebugger.Send("START TEST: Directory exist check:");
            var xTest = Directory.Exists(@"0:\test");
            Assert.IsTrue(xTest, "Folder does not exist!");
            mDebugger.Send("END TEST");

            mDebugger.Send("");

            mDebugger.Send("START TEST: Create Directory");
            var xDirectory = Directory.CreateDirectory(@"0:\test2");
            Assert.IsTrue(xDirectory != null, "Directory.CreateDirectory failed: Directory is null");
            bool xExists = Directory.Exists(@"0:\test2");
            Assert.IsTrue(xExists, "Directory.CreateDirectory failed: Directory doesn't exist after create call");
            mDebugger.Send("END TEST");

            mDebugger.Send("");

            mDebugger.Send("START TEST: Delete a directory:");
            Directory.CreateDirectory(@"0:\TestDir1");
            Assert.IsTrue(Directory.Exists(@"0:\TestDir1"), "TestDir1 wasn't created!");
            Directory.Delete(@"0:\TestDir1");
            Assert.IsFalse(Directory.Exists(@"0:\TestDir1"), "TestDir1 wasn't deleted!");
            mDebugger.Send("END TEST");

            mDebugger.Send("");

#if false
            mDebugger.Send("START TEST: Delete a file with Directory.Delete:");
            File.Create(@"0:\file1.txt");

            try
            {
                Directory.Delete(@"0:\file1.txt");
            }
            catch (Exception e)
            {
                Assert.IsTrue(File.Exists(@"0:\file1.txt"), "The file was deleted by Directory.Delete.");
            }

            mDebugger.Send("END TEST");
            mDebugger.Send("");
#endif

            mDebugger.Send("START TEST: Create a directory with a Long Filename:");
            Directory.CreateDirectory(@"0:\TestDir1");
            Directory.CreateDirectory(@"0:\TestDir1\LongDirectoryName");
            Assert.IsTrue(Directory.Exists(@"0:\TestDir1\LongDirectoryName"), "LongDirectoryName wasn't created!");
            mDebugger.Send("END TEST");

            mDebugger.Send("");

            mDebugger.Send("START TEST: Set Current Directory:");
            var ExpectedCurrentDirectory = @"0:\TestDir1";
            Directory.SetCurrentDirectory(ExpectedCurrentDirectory);
            var CurrentDirectory = Directory.GetCurrentDirectory();
            Assert.IsTrue(ExpectedCurrentDirectory == CurrentDirectory, "Current Directory is wrong!");
            mDebugger.Send("END TEST");

            mDebugger.Send("");
        }
    }
}
