
using System;
using Cosmos.System;
using Cosmos.System.FileSystem.VFS;
using DuNodes.System.Console;
using DuNodes.System.Core;
using DuNodes.System.Extensions;

namespace DuNodes.System.Base
{
    public static class Init
    {
        public static void Initialisation()
        {
            Console.Console.Clear();
           Console.Console.WriteLine(".Booting DuNodes Alpha 0.1 R01", ConsoleColor.Blue, true);
           Console.Console.WriteLine("..Checking prerequisites", ConsoleColor.Blue, true);

            //Init FileSystem
            Console.Console.WriteLine("..Mounting VFS FileSystem", ConsoleColor.Blue, true);
            ENV.myVFS = new SentinelVFS();
            VFSManager.RegisterVFS(myVFS);

            //            var volumes = VFS.GetVolumes();

            //foreach (var directory in volumes)
            //{
            //    Console.Console.WriteLine(directory.Name, ConsoleColor.Blue, true);
            //}

            //Set KeyBoardLayout
            KeyBoardLayout.SwitchKeyLayout(KeyBoardLayout.KeyLayouts.AZERTY);

            //Load settings
            Console.Console.WriteLine("..Loading Settings", ConsoleColor.Blue, true);

            Console.Console.WriteLine("..RAM (32 MB min recommended): " + Extensions.KernelExtensionsHAL.GetMemory() + "", ConsoleColor.Blue, true);
            Console.Console.WriteLine("....INIT OK.....", ConsoleColor.Blue, true);

            
           

        }
    }
}
