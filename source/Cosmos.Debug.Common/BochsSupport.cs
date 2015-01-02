using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

using Microsoft.Win32;

namespace Cosmos.Debug.Common
{
    /// <summary>An helper class that is used from both Cosmos.VS.Package and Cosmos.VS.DebugEngine for
    /// Bochs emulator support.</summary>
    public static class BochsSupport
    {
        static BochsSupport()
        {
            FindBochsExe();
        }

        /// <summary>Get a flag that tell whether Bochs is enabled on this system.</summary>
        public static bool BochsEnabled
        {
          get
          {
            //return (null != BochsExe);
            return false;
          }
        }

        /// <summary>Get a descriptor for the Bochs emulator with debugger support program. The return value
        /// is a null reference if Bochs is not installed.</summary>
        public static FileInfo BochsDebugExe
        {
            get;
            private set;
        }

        /// <summary>Get a descriptor for the Bochs emulator program. The return value is a null reference if
        /// Bochs is not installed.</summary>
        public static FileInfo BochsExe
        {
            get;
            private set;
        }

        /// <summary>Retrieve installation path for Bochs and initialize the <see cref="BochsExe"/> property.
        /// Search is performed using the registry and rely on the shell command defined for the
        /// BochsConfigFile.</summary>
        private static void FindBochsExe()
        {
            try
            {
                using (var runCommandRegistryKey = Registry.ClassesRoot.OpenSubKey(@"BochsConfigFile\shell\Run\command", false))
                {
                    if (null == runCommandRegistryKey) { return; }
                    string commandLine = (string)runCommandRegistryKey.GetValue(null, null);
                    if (null != commandLine) { commandLine = commandLine.Trim(); }
                    if (string.IsNullOrEmpty(commandLine)) { return; }
                    // Now perform some parsing on command line to discover full exe path.
                    string candidateFilePath;
                    int commandLineLength = commandLine.Length;
                    if ('"' == commandLine[0])
                    {
                        // Seek for a non escaped double quote.
                        int lastDoubleQuoteIndex = 1;
                        for (; lastDoubleQuoteIndex < commandLineLength; lastDoubleQuoteIndex++)
                        {
                            if ('"' != commandLine[lastDoubleQuoteIndex]) { continue; }
                            if ('\\' != commandLine[lastDoubleQuoteIndex - 1]) { break; }
                        }
                        if (lastDoubleQuoteIndex >= commandLineLength) { return; }
                        candidateFilePath = commandLine.Substring(1, lastDoubleQuoteIndex - 1);
                    }
                    else
                    {
                        // Seek for first separator character.
                        int firstSeparatorIndex = 0;
                        for (; firstSeparatorIndex < commandLineLength; firstSeparatorIndex++)
                        {
                            if (char.IsSeparator(commandLine[firstSeparatorIndex])) { break; }
                        }
                        if (firstSeparatorIndex >= commandLineLength) { return; }
                        candidateFilePath = commandLine.Substring(0, firstSeparatorIndex);
                    }
                    if (!File.Exists(candidateFilePath)) { return; }
                    BochsExe = new FileInfo(candidateFilePath);
                    BochsDebugExe = new FileInfo(Path.Combine(BochsExe.Directory.FullName, "bochsdbg.exe"));
                    return;
                }
            }
            catch { return; }
        }
    }
}
