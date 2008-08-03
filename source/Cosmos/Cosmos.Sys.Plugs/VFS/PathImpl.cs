using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Indy.IL2CPU.Plugs;

namespace Cosmos.Sys.Plugs {
    [Plug(Target=typeof(Path))]
    public static class PathImpl {

        /// <summary>
        /// Get the directory part of a path to a file. No trailing slash in returned string.
        /// </summary>
        /// <param name="aPath"></param>
        /// <returns></returns>
        public static string GetDirectoryName(string aPath) 
        {
            int xIndex = aPath.LastIndexOfAny(new char[] {'/', '\\'});
            if (xIndex == -1) {
                return aPath;
            }
            return aPath.Substring(0, xIndex);
        }

        public static string GetFileName(string aPath)
        {
            int xIndex = aPath.LastIndexOfAny(new char[] { '/', '\\' });
            if (xIndex == -1)
            {
                return aPath;
            }
            return aPath.Substring(xIndex + 1, aPath.Length - xIndex - 1);
        }
    }
}