using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using SentinelKernel.System.FileSystem.VFS;
using Cosmos.Common.Extensions;
using Sys = Cosmos.System;

namespace SentinelKernel
{
    public class Kernel : Sys.Kernel
    {
        private VFSBase myVFS;

        protected override void BeforeRun()
        {
            Console.WriteLine("Cosmos booted successfully.");
            myVFS = new SentinelVFS();
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
                if (!xTest)
                {
                    Console.WriteLine("Folder does not exist!");
                    return;
                }

                Console.WriteLine("Folder exists!");
                xTest = Directory.Exists("0:\\test\\DirInTest");
                if (!xTest)
                {
                    Console.WriteLine("Subfolder doesn't exist!");
                    return;
                }
                Console.WriteLine("Subfolder exists as well!");

                xTest = File.Exists(@"0:\KudzU.txt");
                if (!xTest)
                {
                    Console.WriteLine(@"\Kudzu.txt not found!");
                    return;
                }

                Console.WriteLine("Kudzu.txt found!");

                xTest = File.Exists(@"0:\Test\DirInTest\Readme.txt");
                if (!xTest)
                {
                    Console.WriteLine(@"\Test\DirInTest\Readme.txt not found!");
                    return;
                }

                Console.WriteLine(@"Test\DirInTest\Readme.txt found!");

                Console.Write("File contents of Kudzu.txt: ");
                Console.WriteLine(File.ReadAllText(@"0:\Kudzu.txt"));
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception occurred:");
                Console.WriteLine(e.Message);
            }
            finally
            {
                while (true)
                    ;
            }
        }
    }
}
