using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.Sys {
    public class Global {
        internal static void Init()
        {
            Console.WriteLine("    Init Virtual File System");
            VFSManager.Init();
        }
    }
}
