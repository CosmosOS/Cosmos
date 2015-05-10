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
            }
            catch (Exception E)
            {
                Console.WriteLine("Exception occurred:");
                Console.WriteLine(E.Message);
            }
            //bool xTest = Directory.Exists("0:\\test");
            //Console.WriteLine("After test");
            //if (xTest)
            //{
            //    Console.WriteLine("Folder exists!");
            //}
            //else
            //{
            //    Console.WriteLine("Folder does not exist!");
            //}
            Stop();
        }
    }
}
