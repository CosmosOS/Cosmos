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
            string message;
            object result, expectedResult;

            // Path.ChangeExtension(string, string)
            mDebugger.Send("START TEST");
            result = Path.ChangeExtension(@"0:\Kudzu.txt", ".doc");
            expectedResult = @"0:\Kudzu.doc";
            message = "Path.ChangeExtenstion (no dot) failed.";
            Assert.IsTrue(result == expectedResult, message);
            mDebugger.Send("END TEST");
            mDebugger.Send("");

            mDebugger.Send("START TEST");
            result = Path.ChangeExtension(@"0:\Kudzu.txt", "doc");
            expectedResult = @"0:\Kudzu.doc";
            message = "Path.ChangeExtenstion (no dot) failed.";
            Assert.IsTrue(result == expectedResult, message);
            mDebugger.Send("END TEST");
            mDebugger.Send("");

            // Path.Combine(string, string)
            mDebugger.Send("START TEST");
            result = Path.Combine(@"0:\", "test");
            expectedResult = @"0:\test";
            message = "Path.Combine (root and directory) failed.";
            Assert.IsTrue(result == expectedResult, message);
            mDebugger.Send("END TEST");
            mDebugger.Send("");

            mDebugger.Send("START TEST");
            result = Path.Combine(@"0:\", "test.txt");
            expectedResult = @"0:\test.txt";
            message = "Path.Combine (root and file) failed.";
            Assert.IsTrue(result == expectedResult, message);
            mDebugger.Send("END TEST");
            mDebugger.Send("");

            mDebugger.Send("START TEST");
            result = Path.Combine(@"0:\test", "test2");
            expectedResult = @"0:\test\test2";
            message = "Path.Combine (directory and directory) failed.";
            Assert.IsTrue(result == expectedResult, message);
            mDebugger.Send("END TEST");
            mDebugger.Send("");

            mDebugger.Send("START TEST");
            result = Path.Combine(@"0:\test", "test2.txt");
            expectedResult = @"0:\test\test2.txt";
            message = "Path.Combine (directory and file) failed.";
            Assert.IsTrue(result == expectedResult, message);
            mDebugger.Send("END TEST");
            mDebugger.Send("");

            // Path.GetDirectoryName(string)
            mDebugger.Send("START TEST");
            result = Path.GetDirectoryName(@"0:\");
            expectedResult = null;
            message = "Path.GetDirectoryName (root) failed.";
            Assert.IsTrue(result == expectedResult, message);
            mDebugger.Send("END TEST");
            mDebugger.Send("");

            mDebugger.Send("START TEST");
            result = Path.GetDirectoryName(@"0:\test");
            expectedResult = @"0:\";
            message = "Path.GetDirectoryName (directory no trailing directory separator) failed.";
            Assert.IsTrue(result == expectedResult, message);
            mDebugger.Send("END TEST");
            mDebugger.Send("");

            mDebugger.Send("START TEST");
            result = Path.GetDirectoryName(@"0:\test\");
            expectedResult = @"0:\";
            message = "Path.GetDirectoryName (directory with trailing directory separator) failed.";
            Assert.IsTrue(result == expectedResult, message);
            mDebugger.Send("END TEST");
            mDebugger.Send("");

            mDebugger.Send("START TEST");
            result = Path.GetDirectoryName(@"0:\test\test2");
            expectedResult = @"0:\test";
            message = "Path.GetDirectoryName (subdirectory no trailing directory separator) failed.";
            Assert.IsTrue(result == expectedResult, message);
            mDebugger.Send("END TEST");
            mDebugger.Send("");

            mDebugger.Send("START TEST");
            result = Path.GetDirectoryName(@"0:\test\test2\");
            expectedResult = @"0:\test";
            message = "Path.GetDirectoryName (subdirectory with trailing directory separator) failed.";
            Assert.IsTrue(result == expectedResult, message);
            mDebugger.Send("END TEST");
            mDebugger.Send("");

            mDebugger.Send("START TEST");
            result = Path.GetDirectoryName(@"0:\test\ .");
            expectedResult = @"0:\test";
            message = "Path.GetDirectoryName (directory with trailing directory separator and invalid path) failed.";
            Assert.IsTrue(result == expectedResult, message);
            mDebugger.Send("END TEST");
            mDebugger.Send("");

            // Path.GetExtension(string)
            mDebugger.Send("START TEST");
            result = Path.GetExtension(@"file");
            expectedResult = string.Empty;
            message = "Path.GetExtension (file no extension) failed.";
            Assert.IsTrue(result == expectedResult, message);
            mDebugger.Send("END TEST");
            mDebugger.Send("");

            mDebugger.Send("START TEST");
            result = Path.GetExtension(@"file.txt");
            expectedResult = "txt";
            message = "Path.GetExtension (file with extension) failed.";
            Assert.IsTrue(result == expectedResult, message);
            mDebugger.Send("END TEST");
            mDebugger.Send("");

            // Path.GetFileName(string aPath)
            mDebugger.Send("START TEST");
            result = Path.GetFileName(@"0:\file");
            expectedResult = string.Empty;
            message = "Path.GetFileName (file no extension) failed.";
            Assert.IsTrue(result == expectedResult, message);
            mDebugger.Send("END TEST");
            mDebugger.Send("");

            mDebugger.Send("START TEST");
            result = Path.GetFileName(@"0:\file.txt");
            expectedResult = "file.txt";
            message = "Path.GetFileName (file with extension) failed.";
            Assert.IsTrue(result == expectedResult, message);
            mDebugger.Send("END TEST");
            mDebugger.Send("");

            mDebugger.Send("START TEST");
            result = Path.GetFileName(@"0:\test\file");
            expectedResult = string.Empty;
            message = "Path.GetFileName (directory and file no extension) failed.";
            Assert.IsTrue(result == expectedResult, message);
            mDebugger.Send("END TEST");
            mDebugger.Send("");

            mDebugger.Send("START TEST");
            result = Path.GetFileName(@"0:\test\file.txt");
            expectedResult = "file.txt";
            message = "Path.GetFileName (directory and file with extension) failed.";
            Assert.IsTrue(result == expectedResult, message);
            mDebugger.Send("END TEST");
            mDebugger.Send("");

            // Path.GetFileNameWithoutExtension(string aPath)
            mDebugger.Send("START TEST");
            result = Path.GetFileNameWithoutExtension(@"0:\file");
            expectedResult = string.Empty;
            message = "Path.GetFileNameWithoutExtension (file no extension) failed.";
            Assert.IsTrue(result == expectedResult, message);
            mDebugger.Send("END TEST");
            mDebugger.Send("");

            mDebugger.Send("START TEST");
            result = Path.GetFileNameWithoutExtension(@"0:\file.txt");
            expectedResult = "file";
            message = "Path.GetFileNameWithoutExtension (file with extension) failed.";
            Assert.IsTrue(result == expectedResult, message);
            mDebugger.Send("END TEST");
            mDebugger.Send("");

            mDebugger.Send("START TEST");
            result = Path.GetFileNameWithoutExtension(@"0:\test\file");
            expectedResult = string.Empty;
            message = "Path.GetFileNameWithoutExtension (directory and file no extension) failed.";
            Assert.IsTrue(result == expectedResult, message);
            mDebugger.Send("END TEST");
            mDebugger.Send("");

            mDebugger.Send("START TEST");
            result = Path.GetFileNameWithoutExtension(@"0:\test\file.txt");
            expectedResult = "file";
            message = "Path.GetFileNameWithoutExtension (directory and file with extension) failed.";
            Assert.IsTrue(result == expectedResult, message);
            mDebugger.Send("END TEST");
            mDebugger.Send("");

            // Path.GetFullPath(string)

            // Path.GetInvalidFileNameChars()
            mDebugger.Send("START TEST");
            result = Path.GetInvalidFileNameChars().Length;
            expectedResult = 0;
            message = "Path.GetInvalidFileNameChars failed.";
            Assert.IsFalse(result == expectedResult, message);
            mDebugger.Send("END TEST");
            mDebugger.Send("");

            // Path.GetInvalidPathChars()
            mDebugger.Send("START TEST");
            result = Path.GetInvalidPathChars().Length;
            expectedResult = 0;
            message = "Path.GetInvalidPathChars failed.";
            Assert.IsFalse(result == expectedResult, message);
            mDebugger.Send("END TEST");
            mDebugger.Send("");

            // Path.GetPathRoot(string)

            // Path.GetRandomFileName()
            mDebugger.Send("START TEST");
            result = Path.GetRandomFileName();
            expectedResult = "random.tmp";
            message = "Path.GetRandomFileName failed.";
            Assert.IsTrue(result == expectedResult, message);
            mDebugger.Send("END TEST");
            mDebugger.Send("");

            // Path.GetTempFileName()
            mDebugger.Send("START TEST");
            result = Path.GetTempFileName();
            expectedResult = "tempfile.tmp";
            message = "Path.GetTempFileName failed.";
            Assert.IsTrue(result == expectedResult, message);
            mDebugger.Send("END TEST");
            mDebugger.Send("");

            // Path.GetTempPath();
            mDebugger.Send("START TEST");
            result = Path.GetTempPath();
            expectedResult = @"\Temp";
            message = "Path.GetTempPath failed.";
            Assert.IsTrue(result == expectedResult, message);
            mDebugger.Send("END TEST");
            mDebugger.Send("");

            // Path.HasExtension(string)

            // Path.IsPathRooted(string)
        }

        private void TestFile()
        {
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
            //using (var xFS = new FileStream(@"0:\Kudzu.txt", FileMode.Create))
            //{
            //    mDebugger.Send("Start writing");
            //    var xStr = "Test FAT Write.";
            //    var xBuff = xStr.GetUtf8Bytes(0, (uint)xStr.Length);
            //    xFS.Write(xBuff, 0, xBuff.Length);
            //    mDebugger.Send("---- Data written");
            //    xFS.Position = 0;
            //    xFS.Read(xBuff, 0, xBuff.Length);
            //    mDebugger.Send(xBuff.GetUtf8String(0, (uint)xBuff.Length));
            //}
        }
    }
}
