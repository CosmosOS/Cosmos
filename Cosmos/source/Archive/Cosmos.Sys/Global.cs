using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.Sys {
    public class Global {
        static public void Init()
        {
            Console.WriteLine("    Init Virtual File System");
            VFSManager.Init();
        }
    }
}
