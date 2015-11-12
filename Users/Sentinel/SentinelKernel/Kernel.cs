using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Cosmos.Common.Extensions;
using Cosmos.System.FileSystem.VFS;
using Cosmos.TestRunner;
using Sys = Cosmos.System;

namespace SentinelKernel
{
    using Cosmos.System.FileSystem;

    public class Kernel : Sys.Kernel
    {
        private VFSBase myVFS;

        protected override void BeforeRun()
        {
            Console.WriteLine("Cosmos booted successfully.");
            myVFS = new CosmosVFS();
            VFSManager.RegisterVFS(myVFS);
        }

        protected override void Run()
        {
            Console.WriteLine("Run");
            try
            {
                var xRoot = Path.GetPathRoot(@"0:\test");
                bool xTest = Directory.Exists("0:\\test");
                Console.WriteLine("After test");
                Assert.IsTrue(xTest, "Folder does not exist!");

                Console.WriteLine("Folder exists!");
                xTest = Directory.Exists("0:\\test\\DirInTest");
                Assert.IsTrue(xTest, "Subfolder doesn't exist!");
                
                xTest = File.Exists(@"0:\KudzU.txt");
                Assert.IsTrue(xTest, @"\Kudzu.txt not found!");

                Console.WriteLine("Kudzu.txt found!");
                Console.Write("File contents of Kudzu.txt: ");
                Console.WriteLine(File.ReadAllText(@"0:\Kudzu.txt"));
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
                Stop();
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception occurred:");
                Console.WriteLine(e.Message);
                
            }
        }
    }
}
