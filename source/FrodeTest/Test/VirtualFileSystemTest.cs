using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cosmos.Sys;
using Cosmos.Sys.FileSystem;
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


            //Check directory exists
            //Cosmos.Hardware.DebugUtil.SendMessage("VFSTest.cs", "Check directory exists");
            //if (Directory.Exists("/0/Illegal/"))
            //    Console.WriteLine("Fail3");
            //if (!Directory.Exists("/0/Zulu"))
            //    Console.WriteLine("Fail4");
            //if (!Directory.Exists("/0/Zulu/"))
            //    Console.WriteLine("Fail5");
            Check.Text = "/0/Illegal/";
            Check.Validate(!Directory.Exists(Check.Text));
            Check.Text = "/0/Zulu";
            Check.Validate(Directory.Exists(Check.Text));
            Check.Text = "/0/Zulu/";
            Check.Validate(Directory.Exists(Check.Text));

            //Check file exists
            //Cosmos.Hardware.DebugUtil.SendMessage("VFSTest.cs", "Check file exists");
            //if (!File.Exists("/0/Readme.txt"))
            //    Console.WriteLine("Fail-File1");
            //if (File.Exists("/0/BogusFile.txt"))
            //    Console.WriteLine("Fail-File2");
            //if (!File.Exists("/0/Alfa/Bravo/slide_puzzle.lua"))
            //    Console.WriteLine("Fail-File3");
            Check.Text = "/0/Readme.txt";
            Check.Validate(File.Exists(Check.Text));
            Check.Text = "/0/BogusFile.txt";
            Check.Validate(!File.Exists(Check.Text));
            Check.Text = "/0/Alfa/Bravo/slide_puzzle.lua";
            Check.Validate(File.Exists(Check.Text));


            //Current directory
            //Cosmos.Hardware.DebugUtil.SendMessage("VFSTest.cs", "Testing Current Directory");
            //Console.WriteLine("Current directory: " + Directory.GetCurrentDirectory());
            //Directory.SetCurrentDirectory("/0/Alfa/Bravo/");
            //Console.WriteLine("Current directory: " + Directory.GetCurrentDirectory());
            Check.Text = "/0/Alfa/Bravo/";
            Directory.SetCurrentDirectory(Check.Text);
            Check.Validate(Directory.GetCurrentDirectory() == Check.Text);

            //Enumerate directories
            Cosmos.Hardware.DebugUtil.SendMessage("VFSTest.cs", "Enumerating directories");
            foreach (string xDir in Directory.GetDirectories(Directory.GetCurrentDirectory()))
            {
                Cosmos.Hardware.DebugUtil.SendMessage("VFSTest.cs", "Enumerating directory " + xDir);
                Console.WriteLine(xDir);
            }
            //Enumerate files
            Cosmos.Hardware.DebugUtil.SendMessage("VFSTest.cs", "Enumerating files");
            foreach (string xFile in Directory.GetFiles(Directory.GetCurrentDirectory()))
            {
                Console.WriteLine(xFile);
            }

            //Get specific directory
            Cosmos.Hardware.DebugUtil.SendMessage("VFSTest.cs", "Get specific directory");
            DirectoryInfo xDirInfo = new DirectoryInfo(Directory.GetCurrentDirectory());
            Console.Write("Got directory : " + xDirInfo.Name);
            Cosmos.Hardware.TextScreen.NewLine();

            //Check relative and absolute paths
            Cosmos.Hardware.DebugUtil.SendMessage("VFSTest.cs", "Check relative and absolute paths");
            if (!Path.IsPathRooted("0:/Alfa/"))
                Console.WriteLine("Fail1");
            if (Path.IsPathRooted("Alfa"))
                Console.WriteLine("Fail2");

            Console.WriteLine("GetDirectoryName: " + Path.GetDirectoryName("/0/Zulu"));
            Console.WriteLine("GetDirectoryName: " + Path.GetDirectoryName("/0/Zulu/"));

            Cosmos.Hardware.DebugUtil.SendMessage("VFSTest.cs", "Splitting");
            //SplitPath
            string xPathToSplit = @"0:\Alfa\Bravo\Charlie\";
            string[] xParts = VFSManager.SplitPath(xPathToSplit);
            Console.WriteLine("Splitting " + xPathToSplit + " into: ");
            for (int i = 0; i < xParts.Length; i++)
            {
                Console.WriteLine(xParts[i]);
            }

            //if (!Directory.Exists("0:/Frode/"))
            //    Console.WriteLine("Fail6");
            //Change directory
            //Enumerate directory

            //Enumerate drives
            Cosmos.Hardware.DebugUtil.SendMessage("VFSTest.cs", "Enumerate drives");
            Console.WriteLine("Drives found:");
            foreach (string drive in Directory.GetLogicalDrives())
                Console.WriteLine("    " + drive);

            //GetPathRoot
            Cosmos.Hardware.DebugUtil.SendMessage("VFSTest.cs", "Testing GetPathRoot");
            Console.WriteLine("PathRoot (1:/): " + Path.GetPathRoot("1:/Alfa/"));
            Console.WriteLine("PathRoot (blank): " + Path.GetPathRoot("ShouldBeBlank"));

            //Console.WriteLine("Directory separator: " + Path.DirectorySeparatorChar.ToString());
            //Console.WriteLine("Directory separator (alternative): " + Path.AltDirectorySeparatorChar.ToString());

            ////Console.WriteLine("Contents of Readme.txt: " + File.ReadAllText("/0/Readme.txt"));

            ////if (Directory.Exists(@"/0/lost+found/"))
            ////    Console.WriteLine(@"Found lost+found");
            ////else
            ////    Console.WriteLine(@"Unable to find lost+found");


            //Console.WriteLine("1:/ " + "1:/".IsADriveVolume());
            //Console.WriteLine("1:/test" + "1:/".IsADriveVolume());

            ////FilesystemEntry xAlfaDir = VFSManager.GetDirectoryEntry("/0/Alfa/Bravo/");
            ////Console.WriteLine("Alfadir: " + xAlfaDir.Name);
            ////foreach (FilesystemEntry xEntry in VFSManager.GetDirectoryListing(xAlfaDir))
            ////{
            ////    Console.WriteLine(xEntry.Name);
            ////}
        }
    }
}
