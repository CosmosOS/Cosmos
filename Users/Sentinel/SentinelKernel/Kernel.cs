using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using SentinelKernel.System.FileSystem.VFS;
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

                xTest = File.Exists(@"0:\Kudzu.txt");
                if (!xTest)
                {
                    Console.WriteLine(@"\Kudzu.txt not found!");
                    return;
                }

                Console.WriteLine("Kudzu.txt found!");

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
