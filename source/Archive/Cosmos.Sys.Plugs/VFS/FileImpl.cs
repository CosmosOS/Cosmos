using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Cosmos.IL2CPU.Plugs;

namespace Cosmos.Sys.Plugs {
    [Plug(Target=typeof(File))]
    public static class FileImpl {
        public static bool Exists(string aFile) {
            return VFSManager.FileExists(aFile);
        }

        public static string ReadAllText(string aFile)
        {
            //Find file
            if (!File.Exists(aFile))
                return "Could not find file " + aFile;
                //throw new FileNotFoundException("Could not find file " + aFile);

            return VFSManager.ReadFileAsString(aFile);
        }
    }
}