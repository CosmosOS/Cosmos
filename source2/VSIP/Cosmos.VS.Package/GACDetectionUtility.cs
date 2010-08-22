using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Win32;
using System.IO;

namespace Cosmos.VS.Package
{
    /// <summary>
    /// This class exists, because our project system cant determine whether assembly references come from stack, just by the info given by vs.
    /// We detect this by:
    /// + checking whether the directory the assembly is in the HKLM\Software\Microsoft\.NETFramework\v2.0.50727\AssemblyFoldersEx\*\@ keys
    /// </summary>
    public static class GACDetectionUtility
    {
        private static List<string> mGACDirs = null;

        public static bool IsAssemblyFileFromGAC(string filename)
        {
            if (String.IsNullOrEmpty(filename)) {
                // if filename is empty, assume gac as well
                return true;
            }
            if (mGACDirs == null)
            {
                #region initialize
                mGACDirs = new List<string>();
                using (var xRegKey = Registry.LocalMachine.OpenSubKey(@"Software\Microsoft\.NETFramework\v2.0.50727\AssemblyFoldersEx", false))
                {
                    foreach (var xSubKeyName in xRegKey.GetSubKeyNames())
                    {
                        using (var xSubKey = xRegKey.OpenSubKey(xSubKeyName, false))
                        {
                            var xDirValue = (string)xSubKey.GetValue(null);
                            if (!xDirValue.EndsWith("\\"))
                            {
                                xDirValue += "\\";
                            }
                            if (!mGACDirs.Contains(xDirValue))
                            {
                                mGACDirs.Add(xDirValue);
                            }
                        }
                    }
                }
                #endregion
            }
            var xDirPart = Path.GetDirectoryName(filename);
            if (!xDirPart.EndsWith("\\"))
            {
                xDirPart += "\\";
            }
            return mGACDirs.Contains(xDirPart);
        }
    }
}