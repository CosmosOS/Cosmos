using System;
using System.IO;

using Cosmos.Common.Extensions;
using Sys = Cosmos.System;
using Cosmos.System.FileSystem;
using Cosmos.System.FileSystem.VFS;
using Cosmos.TestRunner;

namespace Cosmos.Kernel.Tests.FileSystemPlugs
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

        private global::Cosmos.Debug.Kernel.Debugger mDebugger = new global::Cosmos.Debug.Kernel.Debugger("User", "Test");

        protected override void Run()
        {
            try
            {
                mDebugger.Send("Run");

                TestPathPlugs();

                TestController.Completed();
            }
            catch (Exception E)
            {
                mDebugger.Send("Exception occurred");
                mDebugger.Send(E.Message);
                TestController.Failed();
            }
        }

        private void TestPathPlugs()
        {
            mDebugger.Send("-- START TestPathPlugs --");

            // Path.ChangeExtension(string, string)
            RunEQTest(Path.ChangeExtension(@"0:\Kudzu.txt", ".doc"), @"0:\Kudzu.doc", "Path.ChangeExtenstion (with dot) failed.");
            RunEQTest(Path.ChangeExtension(@"0:\Kudzu.txt", "doc"), @"0:\Kudzu.doc", "Path.ChangeExtenstion (no dot) failed.");

            // Path.Combine(string, string)
            RunEQTest(Path.Combine(@"0:\", "test"), @"0:\test", "Path.Combine (root and directory) failed.");
            RunEQTest(Path.Combine(@"0:\", "test.txt"), @"0:\test.txt", "Path.Combine (root and file) failed.");
            RunEQTest(Path.Combine(@"0:\test", "test2"), @"0:\test\test2", "Path.Combine (directory and directory) failed.");
            RunEQTest(Path.Combine(@"0:\test", "test2.txt"), @"0:\test\test2.txt", "Path.Combine (directory and file) failed.");

            // Path.GetDirectoryName(string)
            RunEQTest(Path.GetDirectoryName(@"0:\"), null, "Path.GetDirectoryName (root) failed.");
            RunEQTest(Path.GetDirectoryName(@"0:\test"), @"0:\", "Path.GetDirectoryName (directory no trailing directory separator) failed.");
            RunEQTest(Path.GetDirectoryName(@"0:\test\"), @"0:\", "Path.GetDirectoryName (directory with trailing directory separator) failed.");
            RunEQTest(Path.GetDirectoryName(@"0:\test\test2"), @"0:\test", "Path.GetDirectoryName (subdirectory no trailing directory separator) failed.");
            RunEQTest(Path.GetDirectoryName(@"0:\test\test2\"), @"0:\test", "Path.GetDirectoryName (subdirectory with trailing directory separator) failed.");
            RunEQTest(Path.GetDirectoryName(@"0:\test\ ."), @"0:\test", "Path.GetDirectoryName (directory with trailing directory separator and invalid path) failed.");

            // Path.GetExtension(string)
            RunEQTest(Path.GetExtension(@"file"), string.Empty, "Path.GetExtension (file no extension) failed.");
            RunEQTest(Path.GetExtension(@"file.txt"), "txt", "Path.GetExtension (file with extension) failed.");

            // Path.GetFileName(string aPath)
            RunEQTest(Path.GetFileName(@"0:\file"), string.Empty, "Path.GetFileName (file no extension) failed.");
            RunEQTest(Path.GetFileName(@"0:\file.txt"), "file.txt", "Path.GetFileName (file with extension) failed.");
            RunEQTest(Path.GetFileName(@"0:\test\file"), string.Empty, "Path.GetFileName (directory and file no extension) failed.");
            RunEQTest(Path.GetFileName(@"0:\test\file.txt"), "file.txt", "Path.GetFileName (directory and file with extension) failed.");

            // Path.GetFileNameWithoutExtension(string aPath)
            RunEQTest(Path.GetFileNameWithoutExtension(@"0:\file"), string.Empty, "Path.GetFileNameWithoutExtension (file no extension) failed.");
            RunEQTest(Path.GetFileNameWithoutExtension(@"0:\file.txt"), "file", "Path.GetFileNameWithoutExtension (file with extension) failed.");
            RunEQTest(Path.GetFileNameWithoutExtension(@"0:\test\file"), string.Empty, "Path.GetFileNameWithoutExtension (directory and file no extension) failed.");
            RunEQTest(Path.GetFileNameWithoutExtension(@"0:\test\file.txt"), "file", "Path.GetFileNameWithoutExtension (directory and file with extension) failed.");

            // Path.GetFullPath(string)

            RunGTTest(Path.GetInvalidFileNameChars().Length, 0, "Path.GetInvalidFileNameChars failed.");
            RunGTTest(Path.GetInvalidPathChars().Length, 0, "Path.GetInvalidPathChars failed.");

            // Path.GetPathRoot(string)

            RunEQTest(Path.GetRandomFileName(), "random.tmp", "Path.GetRandomFileName failed.");
            RunEQTest(Path.GetTempFileName(), "tempfile.tmp", "Path.GetTempFileName failed.");
            RunEQTest(Path.GetTempPath(), @"\Temp", "Path.GetTempPath failed.");

            // Path.HasExtension(string)

            // Path.IsPathRooted(string)

            mDebugger.Send("-- END TestPathPlugs --");
        }

        private void RunEQTest(string aActualTestResult, string aExpectedTestResult, string aTestFailedMessage)
        {
            bool xTestResult = aActualTestResult == aExpectedTestResult;
            string xTestCompareString = aExpectedTestResult + " == " + aActualTestResult;
            mDebugger.Send("Test = " + xTestCompareString);
            Assert.IsTrue(xTestResult, aTestFailedMessage);
        }

        private void RunGTTest(int aActualTestResult, int aExpectedTestResult, string aTestFailedMessage)
        {
            bool xTestResult = aActualTestResult > aExpectedTestResult;
            string xTestCompareString = aExpectedTestResult + " > " + aActualTestResult;
            mDebugger.Send("Test: " + xTestCompareString);
            Assert.IsTrue(xTestResult, aTestFailedMessage);
        }
    }
}
