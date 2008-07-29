using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Indy.IL2CPU.Plugs;

namespace Cosmos.Sys.Plugs {
    [Plug(Target=typeof(File))]
    public static class FileImpl {
        public static bool Exists(string aFile) {
            return VFSManager.FileExists(aFile);
        }
    }
}