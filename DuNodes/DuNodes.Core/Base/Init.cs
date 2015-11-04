
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
            Console.Bootscreen.Show("Booting DuNodes Alpha 0.1 R01");
            Console.Bootscreen.Show("Checking prerequisites");

            //Init FileSystem
            Console.Bootscreen.Show("Mounting NFS lite FileSystem");
            DNFS_Helper.DetectDrives();
            Console.Bootscreen.Show("Mounted");

            //Init Settings
            Console.Bootscreen.Show("Init and Load Config");
            Configuration.Configuration.LoadConfiguration();

            //Set KeyBoardLayout
            Console.Bootscreen.Show("Settings Keyboard Layout");
            KeyBoardLayout.SwitchKeyLayoutByString(ENV.currentMapKey);

            //Set env
            Console.Bootscreen.Show("Settings ENV var");
            ENV.currentPath = "/";

            Console.Bootscreen.Show("RAM (32 MB min recommended):" + KernelExtensionsHAL.GetMemory());
            Console.Bootscreen.Show("INIT OK");
        }
    }
}
