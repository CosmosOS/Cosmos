using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Microsoft.Win32;

namespace Cosmos.Debug.VSDebugEngine
{
    public static class PathUtilities
    {
        /// <summary>
        /// Gets the root of the 
        /// </summary>
        /// <returns></returns>
        public static string GetCosmosDir()
        {
            using(var xKey = Registry.LocalMachine.OpenSubKey("Software\\Cosmos")){
                return xKey.GetValue(null).ToString();
            }
            //return Path.GetDirectoryName(Path.GetDirectoryName(Path.GetDirectoryName(typeof(PathUtilities).Assembly.Location)));
        }

        public static string GetBuildDir()
        {
            return Path.Combine(GetCosmosDir(), "Build");
        }

        public static string GetToolsDir()
        {
            return Path.Combine(GetBuildDir(), "Tools");
        }

        public static string GetVSIPDir()
        {
            return Path.Combine(GetBuildDir(), "VSIP");
        }
    }
}