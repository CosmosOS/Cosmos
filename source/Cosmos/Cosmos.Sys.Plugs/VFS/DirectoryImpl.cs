using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Indy.IL2CPU.Plugs;

namespace Cosmos.Sys.Plugs {
    [Plug(Target=typeof(Directory))]
    public static class DirectoryImpl
    {
        public static bool Exists(string aDir) {
            return VFSManager.DirectoryExists(aDir);
        }
    }
}