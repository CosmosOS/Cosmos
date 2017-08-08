using System;
using System.Collections.Generic;
using System.Text;
using Sys = Cosmos.System;
using System.IO;
using zConsole;

namespace FrotzKernel
{
    public class Kernel : Sys.Kernel
    {
        protected override void BeforeRun()
        {
            //Create a new FAT filesystem access object.
            var fs = new Sys.FileSystem.CosmosVFS();
            //Register the access object with the VFSManager, so that System.IO can access it.
            Cosmos.System.FileSystem.VFS.VFSManager.RegisterVFS(fs);
            //Some descriptions...
            Console.WriteLine("Welcome to FrotzCosmos, the Cosmos port of the Frotz.NET emulator for emulating Infocom Z-Machine games.");
            Console.WriteLine("Created by Michael VanOverbeek, with help from mterwoord, Kudzu, and the other Cosmos devs.");
        }

        [ManifestResourceStream(ResourceName = "FrotzKernel.ZORK1.DAT")]
        public readonly static byte[] GameData;

        protected override void Run()
        {
            /*
             * Charles needs to fix FAT for this to work, but basically what it does
             * is it takes input from the user, and if it is 'ls', it lists all
             * files in 0:\\, and if not, it interprets the input as a file path
             * and creates a new zConsole.ZConsoleScreen() instance, which
             * loads the file, and interprets it as a Z-Machine game.
             * If something bad happens, the exception is written to the
             * console and the user is told to specify a path again.
             */

            Console.WriteLine("Specify game path:");
            string fName = null;
            try
            {
                fName = Console.ReadLine();
                if (fName == "ls")
                {
                    foreach (var dir in Directory.GetFiles("0:\\"))
                    {
                        Console.WriteLine($"0:\\{dir}");
                    }
                }
                else
                {
                    var screen = new ZConsoleScreen(fName, GameData);
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}
