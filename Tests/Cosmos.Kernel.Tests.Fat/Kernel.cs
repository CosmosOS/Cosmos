using System;
using System.IO;
using Cosmos.Common.Extensions;
using Cosmos.Debug.Kernel;
using Cosmos.System.FileSystem;
using Cosmos.System.FileSystem.VFS;
using Cosmos.TestRunner;
using Sys = Cosmos.System;

namespace Cosmos.Kernel.Tests.Fat
{
    public class Kernel : Sys.Kernel
    {
        private VFSBase myVFS;

        protected override void BeforeRun()
        {
            Console.WriteLine("Cosmos booted successfully, now start testing");
            myVFS = new CosmosVFS();
            VFSManager.RegisterVFS(myVFS);
        }

        private readonly Debugger mDebugger = new Debugger("User", "Test");

        protected override void Run()
        {
            try
            {
                mDebugger.Send("Run");

                TestPath();
                TestDirectory();
                TestFile();
                TestFileStream();

                TestController.Completed();
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception occurred");
                Console.WriteLine(e.Message);
                TestController.Failed();
            }
        }

        public void TestPath()
        {
            string xMessage;
            string xStringResult, xStringExpectedResult;
            int xIntResult, xIntExpectedResult;

            // Path.ChangeExtension(string, string)
            mDebugger.Send("START TEST");
            xStringResult = Path.ChangeExtension(@"0:\Kudzu.txt", ".doc");
            xStringExpectedResult = @"0:\Kudzu.doc";
            xMessage = "Path.ChangeExtenstion (no dot) failed.";
            Assert.IsTrue(xStringResult == xStringExpectedResult, xMessage);
            mDebugger.Send("END TEST");
            mDebugger.Send("");

            mDebugger.Send("START TEST");
            xStringResult = Path.ChangeExtension(@"0:\Kudzu.txt", "doc");
            xStringExpectedResult = @"0:\Kudzu.doc";
            xMessage = "Path.ChangeExtenstion (no dot) failed.";
            Assert.IsTrue(xStringResult == xStringExpectedResult, xMessage);
            mDebugger.Send("END TEST");
            mDebugger.Send("");

            // Path.Combine(string, string)
            mDebugger.Send("START TEST");
            xStringResult = Path.Combine(@"0:\", "test");
            xStringExpectedResult = @"0:\test";
            xMessage = "Path.Combine (root and directory) failed.";
            Assert.IsTrue(xStringResult == xStringExpectedResult, xMessage);
            mDebugger.Send("END TEST");
            mDebugger.Send("");

            mDebugger.Send("START TEST");
            xStringResult = Path.Combine(@"0:\", "test.txt");
            xStringExpectedResult = @"0:\test.txt";
            xMessage = "Path.Combine (root and file) failed.";
            Assert.IsTrue(xStringResult == xStringExpectedResult, xMessage);
            mDebugger.Send("END TEST");
            mDebugger.Send("");

            mDebugger.Send("START TEST");
            xStringResult = Path.Combine(@"0:\test", "test2");
            xStringExpectedResult = @"0:\test\test2";
            xMessage = "Path.Combine (directory and directory) failed.";
            Assert.IsTrue(xStringResult == xStringExpectedResult, xMessage);
            mDebugger.Send("END TEST");
            mDebugger.Send("");

            mDebugger.Send("START TEST");
            xStringResult = Path.Combine(@"0:\test", "test2.txt");
            xStringExpectedResult = @"0:\test\test2.txt";
            xMessage = "Path.Combine (directory and file) failed.";
            Assert.IsTrue(xStringResult == xStringExpectedResult, xMessage);
            mDebugger.Send("END TEST");
            mDebugger.Send("");

            // Path.GetDirectoryName(string)
            mDebugger.Send("START TEST");
            xStringResult = Path.GetDirectoryName(@"0:\");
            xStringExpectedResult = null;
            xMessage = "Path.GetDirectoryName (root) failed.";
            Assert.IsTrue(xStringResult == xStringExpectedResult, xMessage);
            mDebugger.Send("END TEST");
            mDebugger.Send("");

            mDebugger.Send("START TEST");
            xStringResult = Path.GetDirectoryName(@"0:\test");
            xStringExpectedResult = @"0:\";
            xMessage = "Path.GetDirectoryName (directory no trailing directory separator) failed.";
            Assert.IsTrue(xStringResult == xStringExpectedResult, xMessage);
            mDebugger.Send("END TEST");
            mDebugger.Send("");

            mDebugger.Send("START TEST");
            xStringResult = Path.GetDirectoryName(@"0:\test\");
            xStringExpectedResult = @"0:\";
            xMessage = "Path.GetDirectoryName (directory with trailing directory separator) failed.";
            Assert.IsTrue(xStringResult == xStringExpectedResult, xMessage);
            mDebugger.Send("END TEST");
            mDebugger.Send("");

            mDebugger.Send("START TEST");
            xStringResult = Path.GetDirectoryName(@"0:\test\test2");
            xStringExpectedResult = @"0:\test";
            xMessage = "Path.GetDirectoryName (subdirectory no trailing directory separator) failed.";
            Assert.IsTrue(xStringResult == xStringExpectedResult, xMessage);
            mDebugger.Send("END TEST");
            mDebugger.Send("");

            mDebugger.Send("START TEST");
            xStringResult = Path.GetDirectoryName(@"0:\test\test2\");
            xStringExpectedResult = @"0:\test";
            xMessage = "Path.GetDirectoryName (subdirectory with trailing directory separator) failed.";
            Assert.IsTrue(xStringResult == xStringExpectedResult, xMessage);
            mDebugger.Send("END TEST");
            mDebugger.Send("");

            mDebugger.Send("START TEST");
            xStringResult = Path.GetDirectoryName(@"0:\test\ .");
            xStringExpectedResult = @"0:\test";
            xMessage = "Path.GetDirectoryName (directory with trailing directory separator and invalid path) failed.";
            Assert.IsTrue(xStringResult == xStringExpectedResult, xMessage);
            mDebugger.Send("END TEST");
            mDebugger.Send("");

            // Path.GetExtension(string)
            mDebugger.Send("START TEST");
            xStringResult = Path.GetExtension(@"file");
            xStringExpectedResult = string.Empty;
            xMessage = "Path.GetExtension (file no extension) failed.";
            Assert.IsTrue(xStringResult == xStringExpectedResult, xMessage);
            mDebugger.Send("END TEST");
            mDebugger.Send("");

            mDebugger.Send("START TEST");
            xStringResult = Path.GetExtension(@"file.txt");
            xStringExpectedResult = "txt";
            xMessage = "Path.GetExtension (file with extension) failed.";
            Assert.IsTrue(xStringResult == xStringExpectedResult, xMessage);
            mDebugger.Send("END TEST");
            mDebugger.Send("");

            // Path.GetFileName(string aPath)
            mDebugger.Send("START TEST");
            xStringResult = Path.GetFileName(@"0:\");
            xStringExpectedResult = string.Empty;
            xMessage = "Path.GetFileName (root directory) failed.";
            Assert.IsTrue(xStringResult == xStringExpectedResult, xMessage);
            mDebugger.Send("END TEST");
            mDebugger.Send("");

            mDebugger.Send("START TEST");
            xStringResult = Path.GetFileName(@"0:\file.txt");
            xStringExpectedResult = "file.txt";
            xMessage = "Path.GetFileName (file with extension) failed.";
            Assert.IsTrue(xStringResult == xStringExpectedResult, xMessage);
            mDebugger.Send("END TEST");
            mDebugger.Send("");

            mDebugger.Send("START TEST");
            xStringResult = Path.GetFileName(@"0:\test\file");
            xStringExpectedResult = "file";
            xMessage = "Path.GetFileName (directory and file no extension) failed.";
            Assert.IsTrue(xStringResult == xStringExpectedResult, xMessage);
            mDebugger.Send("END TEST");
            mDebugger.Send("");

            mDebugger.Send("START TEST");
            xStringResult = Path.GetFileName(@"0:\test\file.txt");
            xStringExpectedResult = "file.txt";
            xMessage = "Path.GetFileName (directory and file with extension) failed.";
            Assert.IsTrue(xStringResult == xStringExpectedResult, xMessage);
            mDebugger.Send("END TEST");
            mDebugger.Send("");

            mDebugger.Send("START TEST");
            xStringResult = Path.GetFileName(@"0:\test\dir\");
            xStringExpectedResult = String.Empty;
            xMessage = "Path.GetFileName (two directories and no file) failed.";
            Assert.IsTrue(xStringResult == xStringExpectedResult, xMessage);
            mDebugger.Send("END TEST");
            mDebugger.Send("");

            // Path.GetFileNameWithoutExtension(string aPath)
            mDebugger.Send("START TEST");
            xStringResult = Path.GetFileNameWithoutExtension(@"0:\file");
            xStringExpectedResult = "file";
            xMessage = "Path.GetFileNameWithoutExtension (file no extension) failed.";
            Assert.IsTrue(xStringResult == xStringExpectedResult, xMessage);
            mDebugger.Send("END TEST");
            mDebugger.Send("");

            mDebugger.Send("START TEST");
            xStringResult = Path.GetFileNameWithoutExtension(@"0:\file.txt");
            xStringExpectedResult = "file";
            xMessage = "Path.GetFileNameWithoutExtension (file with extension) failed.";
            Assert.IsTrue(xStringResult == xStringExpectedResult, xMessage);
            mDebugger.Send("END TEST");
            mDebugger.Send("");

            mDebugger.Send("START TEST");
            xStringResult = Path.GetFileNameWithoutExtension(@"0:\test\file");
            xStringExpectedResult = "file";
            xMessage = "Path.GetFileNameWithoutExtension (directory and file no extension) failed.";
            Assert.IsTrue(xStringResult == xStringExpectedResult, xMessage);
            mDebugger.Send("END TEST");
            mDebugger.Send("");

            mDebugger.Send("START TEST");
            xStringResult = Path.GetFileNameWithoutExtension(@"0:\test\file.txt");
            xStringExpectedResult = "file";
            xMessage = "Path.GetFileNameWithoutExtension (directory and file with extension) failed.";
            Assert.IsTrue(xStringResult == xStringExpectedResult, xMessage);
            mDebugger.Send("END TEST");
            mDebugger.Send("");

            mDebugger.Send("START TEST");
            xStringResult = Path.GetFileNameWithoutExtension(@"0:\test\dir\");
            xStringExpectedResult = String.Empty;
            xMessage = "Path.GetFileNameWithoutExtension (two directories and no file) failed.";
            Assert.IsTrue(xStringResult == xStringExpectedResult, xMessage);
            mDebugger.Send("END TEST");
            mDebugger.Send("");

            // Path.GetFullPath(string)

            // Path.GetInvalidFileNameChars()
            mDebugger.Send("START TEST");
            xIntResult = Path.GetInvalidFileNameChars().Length;
            xIntExpectedResult = 0;
            xMessage = "Path.GetInvalidFileNameChars failed.";
            Assert.IsFalse(xIntResult == xIntExpectedResult, xMessage);
            mDebugger.Send("END TEST");
            mDebugger.Send("");

            // Path.GetInvalidPathChars()
            mDebugger.Send("START TEST");
            xIntResult = Path.GetInvalidPathChars().Length;
            xIntExpectedResult = 0;
            xMessage = "Path.GetInvalidPathChars failed.";
            Assert.IsFalse(xIntResult == xIntExpectedResult, xMessage);
            mDebugger.Send("END TEST");
            mDebugger.Send("");

            // Path.GetPathRoot(string)

            // Path.GetRandomFileName()
            mDebugger.Send("START TEST");
            xStringResult = Path.GetRandomFileName();
            xStringExpectedResult = "random.tmp";
            xMessage = "Path.GetRandomFileName failed.";
            Assert.IsTrue(xStringResult == xStringExpectedResult, xMessage);
            mDebugger.Send("END TEST");
            mDebugger.Send("");

            // Path.GetTempFileName()
            mDebugger.Send("START TEST");
            xStringResult = Path.GetTempFileName();
            xStringExpectedResult = "tempfile.tmp";
            xMessage = "Path.GetTempFileName failed.";
            Assert.IsTrue(xStringResult == xStringExpectedResult, xMessage);
            mDebugger.Send("END TEST");
            mDebugger.Send("");

            // Path.GetTempPath();
            mDebugger.Send("START TEST");
            xStringResult = Path.GetTempPath();
            xStringExpectedResult = @"\Temp";
            xMessage = "Path.GetTempPath failed.";
            Assert.IsTrue(xStringResult == xStringExpectedResult, xMessage);
            mDebugger.Send("END TEST");
            mDebugger.Send("");

            // Path.HasExtension(string)

            // Path.IsPathRooted(string)
        }

        private void TestFile()
        {
            // Moved this test here because if not the test can be executed only a time!
            //  mDebugger.Send("Write to file now");
            File.WriteAllText(@"0:\Kudzu.txt", "Hello Cosmos");
            mDebugger.Send("Text written");

            //
            mDebugger.Send("File contents of Kudzu.txt: ");
            string xContents = File.ReadAllText(@"0:\Kudzu.txt");
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
            var xFile = File.Create(@"0:\test2.txt");
            Assert.IsTrue(xFile != null, "Failed to create a new file.");
            bool xFileExists = File.Exists(@"0:\test2.txt");
            Assert.IsTrue(xFileExists, "Failed to create a new file.");
            mDebugger.Send("END TEST");
            mDebugger.Send("");
        }

        private void TestDirectory()
        {
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
            mDebugger.Send("Get files:");
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
            mDebugger.Send("Get directories:");
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
            mDebugger.Send("Directory exist check:");
            var xTest = Directory.Exists(@"0:\test");
            Assert.IsTrue(xTest, "Folder does not exist!");
            mDebugger.Send("END TEST");
            mDebugger.Send("");

            //
            mDebugger.Send("START TEST: Create directory:");
            var xDirectory = Directory.CreateDirectory(@"0:\test2");
            Assert.IsTrue(xDirectory != null, "Failed to create a new directory.");
            bool xExists = Directory.Exists(@"0:\test2");
            Assert.IsTrue(xExists, "Failed to create a new directory.");
            mDebugger.Send("END TEST");
            mDebugger.Send("");
        }

        private void TestFileStream()
        {
            using (var xFS = new FileStream(@"0:\Kudzu.txt", FileMode.Create))
            {
                mDebugger.Send("START TEST: Filestream:");
                mDebugger.Send("Start writing");
                var xStr = "Test FAT Write.";
                Byte[] xWriteBuff = xStr.GetUtf8Bytes(0, (uint)xStr.Length);
                xFS.Write(xWriteBuff, 0, xWriteBuff.Length);
                mDebugger.Send("---- Data written");
                xFS.Position = 0;
                Byte[] xReadBuff = new byte[xWriteBuff.Length];
                xFS.Read(xReadBuff, 0, xWriteBuff.Length);
                mDebugger.Send("xWriteBuff " + xWriteBuff.GetUtf8String(0, (uint)xWriteBuff.Length) + " xReadBuff " + xReadBuff.GetUtf8String(0, (uint)xWriteBuff.Length));
                String xWriteBuffAsString = xWriteBuff.GetUtf8String(0, (uint)xWriteBuff.Length);
                String xReadBuffAsString = xReadBuff.GetUtf8String(0, (uint)xReadBuff.Length);
                Assert.IsTrue(xWriteBuffAsString == xReadBuffAsString, "Failed to write and read file");
                mDebugger.Send("END TEST");
            }
        }
    }
}
