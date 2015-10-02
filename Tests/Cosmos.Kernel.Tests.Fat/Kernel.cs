using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Cosmos.Debug.Kernel;
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
            Console.WriteLine("Cosmos booted successfully.");
            myVFS = new Sys.SentinelVFS();
            VFSManager.RegisterVFS(myVFS);
        }

        private Debugger mDebugger = new Debugger("User", "Test");

        protected override void Run()
        {
            mDebugger.Send("Run");
            //var xRoot = Path.GetPathRoot(@"0:\test");
            //bool xTest = Directory.Exists("0:\\test");
            //Console.WriteLine("After test");
            //Assert.IsTrue(xTest, "Folder does not exist!");

            //Console.WriteLine("Folder exists!");
            //xTest = Directory.Exists("0:\\test\\DirInTest");
            //Assert.IsTrue(xTest, "Subfolder doesn't exist!");

            //var xTest = File.Exists(@"0:\Kudzu.txt");
            //Assert.IsTrue(xTest, @"\Kudzu.txt not found!");

            mDebugger.Send("File contents of Kudzu.txt: ");
            var xContents = File.ReadAllText(@"0:\Kudzu.txt");
            mDebugger.Send("Contents retrieved");
            mDebugger.Send(xContents);
            //                File.WriteAllText(@"0:\Kudzu.txt", "Test FAT write.");
            //                Console.WriteLine(File.ReadAllText(@"0:\Kudzu.txt"));

            //xTest = File.Exists(@"0:\Test\DirInTest\Readme.txt");
            //if (!xTest)
            //{
            //    Console.WriteLine(@"\Test\DirInTest\Readme.txt not found!");
            //    return;
            //}

            //Console.WriteLine(@"Test\DirInTest\Readme.txt found!");

            //Console.WriteLine(@"File contents of Test\DirInTest\Readme.txt: ");
            //Console.WriteLine(File.ReadAllText(@"0:\Test\DirInTest\Readme.txt"));

        }
    }
}
