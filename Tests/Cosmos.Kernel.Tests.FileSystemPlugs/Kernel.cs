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

            string xResultString = string.Empty;
            char[] xResultCharArray = new char[0];

            xResultString = Path.ChangeExtension(@"0:\Kudzu.txt", ".doc");
            Assert.IsTrue(xResultString == @"0:\Kudzu.doc", "Path.ChangeExtenstion (with dot) failed.");

            xResultString = Path.ChangeExtension(@"0:\Kudzu.txt", "doc");
            Assert.IsTrue(xResultString == @"0:\Kudzu.doc", "Path.ChangeExtenstion (no dot) failed.");


            xResultString = Path.Combine(@"0:\", "test");
            Assert.IsTrue(xResultString == @"0:\test", "Path.Combine (root and directory) failed.");

            xResultString = Path.Combine(@"0:\", "test.txt");
            Assert.IsTrue(xResultString == @"0:\test.txt", "Path.Combine (root and file) failed.");

            xResultString = Path.Combine(@"0:\test", "test2");
            Assert.IsTrue(xResultString == @"0:\test\test2", "Path.Combine (directory and directory) failed.");

            xResultString = Path.Combine(@"0:\test", "test2.txt");
            Assert.IsTrue(xResultString == @"0:\test\test2.txt", "Path.Combine (directory and file) failed.");

            //Path.GetDirectoryName(string aPath)

            //Path.GetExtension(string aPath)

            //Path.GetFileName(string aPath)

            //Path.GetFileNameWithoutExtension(string aPath)

            //Path.GetFullPath(string aPath)

            xResultCharArray = Path.GetInvalidFileNameChars();
            Assert.IsTrue(xResultCharArray.Length > 0, "Path.GetInvalidFileNameChars failed.");

            xResultCharArray = Path.GetInvalidPathChars();
            Assert.IsTrue(xResultCharArray.Length > 0, "Path.GetInvalidPathChars failed.");

            //Path.GetPathRoot(string aPath)

            xResultString = Path.GetRandomFileName();
            Assert.IsTrue(!string.IsNullOrWhiteSpace(xResultString), "Path.GetRandomFileName failed.");

            xResultString = Path.GetTempFileName();
            Assert.IsTrue(!string.IsNullOrWhiteSpace(xResultString), "Path.GetTempFileName failed.");

            xResultString = Path.GetTempPath();
            Assert.IsTrue(!string.IsNullOrWhiteSpace(xResultString), "Path.GetTempPath failed.");

            //Path.HasExtension(string aPath)

            //Path.IsPathRooted(string aPath)

            mDebugger.Send("-- END TestPathPlugs --");
        }
    }
}
