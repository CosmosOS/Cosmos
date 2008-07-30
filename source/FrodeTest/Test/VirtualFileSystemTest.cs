using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cosmos.Sys;
using Cosmos.FileSystem;
using System.IO;

namespace FrodeTest.Test
{
    public class VirtualFileSystemTest
    {
        public static void RunTest()
        {
            //foreach (FilesystemEntry entry in VFSManager.GetDirectoryListing("/1/"))
            //{
            //    Console.WriteLine(entry.Name);
            //}

            Console.WriteLine("Drives found:");
            foreach (string drive in Directory.GetLogicalDrives())
                Console.WriteLine("    " + drive);


            //Console.WriteLine(Directory.Exists("/1/TempDir"));
            Console.WriteLine("About to check Directory.Exists");
            Console.ReadLine();
            if (Directory.Exists(@"/1/TempDir/"))
                Console.WriteLine(@"Found TempDir");
            else
                Console.WriteLine(@"Unable to find TempDir");
        }
    }
}
