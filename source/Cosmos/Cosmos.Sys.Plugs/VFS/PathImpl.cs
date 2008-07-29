using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Indy.IL2CPU.Plugs;

namespace Cosmos.Sys.Plugs {
    [Plug(Target=typeof(Path))]
    public static class PathImpl {
        public static string GetDirectoryName(string aPath) {
            int xIndex = aPath.LastIndexOfAny(new char[] {'/', '\\'});
            if (xIndex == -1) {
                return aPath;
            }
            return aPath.Substring(0,
                                   xIndex);
        }
    }
}