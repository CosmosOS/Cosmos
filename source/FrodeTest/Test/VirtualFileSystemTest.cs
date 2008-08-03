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

            Console.WriteLine("Directory separator: " + Path.DirectorySeparatorChar.ToString());

            //Console.WriteLine("Contents of Readme.txt: " + File.ReadAllText("/0/Readme.txt"));


            //Console.WriteLine(Directory.Exists("/1/TempDir"));
            if (Directory.Exists(@"/0/lost+found/"))
                Console.WriteLine(@"Found TempDir");
            else
                Console.WriteLine(@"Unable to find lost+found");

            FilesystemEntry xAlfaDir = VFSManager.GetDirectoryEntry("/0/Alfa/Bravo/");
            Console.WriteLine("Alfadir: " + xAlfaDir.Name);
            foreach (FilesystemEntry xEntry in VFSManager.GetDirectoryListing(xAlfaDir))
            {
                Console.WriteLine(xEntry.Name);
            }
            
        }
    }
}
