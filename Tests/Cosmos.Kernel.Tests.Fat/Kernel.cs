using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Cosmos.Common.Extensions;
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

        private global::Cosmos.Debug.Kernel.Debugger mDebugger = new global::Cosmos.Debug.Kernel.Debugger("User", "Test");

        protected override void Run()
        {
            try
            {
                mDebugger.Send("Run");

                bool xTest;
                string xContents;

                mDebugger.Send("START TEST: Get parent:");
                var xParent = Directory.GetParent(@"0:\test");
                Assert.IsTrue(xParent != null, "Failed to get directory parent.");
                mDebugger.Send("END TEST");
                mDebugger.Send("");

                mDebugger.Send("START TEST: Get parent:");
                xParent = Directory.GetParent(@"0:\test\");
                Assert.IsTrue(xParent != null, "Failed to get directory parent.");
                mDebugger.Send("END TEST");
                mDebugger.Send("");

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

                Assert.IsTrue(Path.GetDirectoryName(@"0:\test") == @"0:\", @"Path.GetDirectoryName(@'0:\test') == @'0:\'");
                mDebugger.Send("END TEST");
                mDebugger.Send("");

                Assert.IsTrue(Path.GetFileName(@"0:\test") == @"test", @"Path.GetFileName(@'0:\test') == @'test'");
                mDebugger.Send("END TEST");
                mDebugger.Send("");

                mDebugger.Send("File exist check:");
                xTest = File.Exists(@"0:\Kudzu.txt");
                Assert.IsTrue(xTest, @"\Kudzu.txt not found!");
                mDebugger.Send("END TEST");
                mDebugger.Send("");

                mDebugger.Send("Directory exist check:");
                xTest = Directory.Exists(@"0:\test");
                Assert.IsTrue(xTest, "Folder does not exist!");
                mDebugger.Send("END TEST");
                mDebugger.Send("");

                mDebugger.Send("File contents of Kudzu.txt: ");
                xContents = File.ReadAllText(@"0:\Kudzu.txt");
                mDebugger.Send("Contents retrieved");
                mDebugger.Send(xContents);
                Assert.IsTrue(xContents == "Hello Cosmos", "Contents of Kudzu.txt was read incorrectly!");
                mDebugger.Send("END TEST");
                mDebugger.Send("");

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

                using (var xFS = new FileStream(@"0:\Kudzu.txt", FileMode.Create))
                {
                    mDebugger.Send("Start writing");
                    var xStr = "Test FAT Write.";
                    var xBuff = xStr.GetUtf8Bytes(0, (uint)xStr.Length);
                    xFS.Write(xBuff, 0, xBuff.Length);
                    mDebugger.Send("---- Data written");
                    xFS.Position = 0;
                    xFS.Read(xBuff, 0, xBuff.Length);
                    mDebugger.Send(xBuff.GetUtf8String(0, (uint)xBuff.Length));
                }

                mDebugger.Send("Write to file now");
                File.WriteAllText(@"0:\Kudzu.txt", "Test FAT write.");
                mDebugger.Send("Text written");

                xContents = File.ReadAllText(@"0:\Kudzu.txt");
                mDebugger.Send("Contents retrieved after writing");
                mDebugger.Send(xContents);
                Assert.IsTrue(xContents == "Test FAT write.", "Contents of Kudzu.txt was written incorrectly!");
                mDebugger.Send("END TEST");
                mDebugger.Send("");

                mDebugger.Send("START TEST: Create directory:");
                var xDirectory = Directory.CreateDirectory(@"0:\test2");
                Assert.IsTrue(xDirectory != null, "Failed to create a new directory.");
                bool xExists = Directory.Exists(@"0:\test2");
                Assert.IsTrue(xExists, "Failed to create a new directory.");
                mDebugger.Send("END TEST");
                mDebugger.Send("");

                mDebugger.Send("START TEST: Create file:");
                var xFile = File.Create(@"0:\test2.txt");
                Assert.IsTrue(xFile != null, "Failed to create a new file.");
                bool xFileExists = File.Exists(@"0:\test2.txt");
                Assert.IsTrue(xFileExists, "Failed to create a new file.");
                mDebugger.Send("END TEST");
                mDebugger.Send("");
                TestController.Completed();
            }
            catch (Exception E)
            {
                mDebugger.Send("Exception occurred");
                mDebugger.Send(E.Message);
                TestController.Failed();
            }
        }
    }
}
