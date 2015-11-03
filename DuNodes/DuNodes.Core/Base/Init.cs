
using System;
using System.IO;
using System.Linq;
using Cosmos.Common.Extensions;
using Cosmos.System;
using Cosmos.System.FileSystem.VFS;
using DuNodes.HAL.Extensions;
using DuNodes.HAL.FileSystem;
using DuNodes.HAL.FileSystem.Base;
using DuNodes.System.Console;
using DuNodes.System.Core;
using DuNodes.System.Extensions;
using DuNodes.System.FileSystem;

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
            Console.Console.WriteLine("..Mounting NFS lite FileSystem", ConsoleColor.Blue, true);
            DNFS_Helper.DetectDrives();
            Console.Console.WriteLine("..Mounted", ConsoleColor.Blue, true);

            //Init Settings
            Console.Console.WriteLine("..Init and Load Config", ConsoleColor.Blue, true);
            Configuration.Configuration.LoadConfiguration();

            //Set KeyBoardLayout
            Console.Console.WriteLine("..Settings Keyboard Layout + " + ENV.currentMapKey, ConsoleColor.Blue, true);
            KeyBoardLayout.SwitchKeyLayoutByString(ENV.currentMapKey);

            //Set env
            Console.Console.WriteLine("..Settings ENV var", ConsoleColor.Blue, true);
            ENV.currentPath = "/";

            Console.Console.WriteLine("..RAM (32 MB min recommended): " + KernelExtensionsHAL.GetMemory() + "", ConsoleColor.Blue, true);
            Console.Console.WriteLine("....INIT OK.....", ConsoleColor.Blue, true);
        }
    }
}
