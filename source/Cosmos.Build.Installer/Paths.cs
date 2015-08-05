using System;
using Microsoft.Win32;

namespace Cosmos.Build.Installer
{
    public static class Paths
    {
        /// <summary>
        /// Version of Visual Studio for which paths should be detected.
        /// </summary>
        private static VsVersion vsVersion;

        public static readonly string ProgFiles32;
        public static readonly string ProgFiles64;
        public static readonly string Windows;

        static Paths()
        {
            if (Global.IsX64)
            {
                ProgFiles32 = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
                ProgFiles64 = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
            }
            else
            {
                ProgFiles32 = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
            }

            UpdateVsPath();
            Windows = Environment.GetFolderPath(Environment.SpecialFolder.Windows);
        }

        /// <summary>
        /// Gets path where Visual Studio installed
        /// </summary>
        public static string VSInstall { get; private set; }

        /// <summary>
        /// Version of Visual Studio for which paths should be detected.
        /// </summary>
        public static VsVersion VsVersion
        {
            get { return vsVersion; }

            set
            {
                vsVersion = value;
                UpdateVsPath();
            }
        }

        /// <summary>
        /// Updates path to VS version
        /// </summary>
        private static void UpdateVsPath()
        {
            // The Install Dir will pickup only the Visual Studio 2013/2015 path currently.
            RegistryKey key;
            string vsVersionCode;
            switch (VsVersion)
            {
                case VsVersion.Vs2015:
                    vsVersionCode = "14.0";
                    break;
                default:
                    throw new NotSupportedException("Versions of VS other then 2013 and 2015 not supported.");
            }

            var registryKey = string.Format(
                @"SOFTWARE{0}\microsoft\VisualStudio\{1}", 
                Environment.Is64BitOperatingSystem ? @"\Wow6432Node" : "",
                vsVersionCode);
            key = Registry.LocalMachine.OpenSubKey(registryKey);
            VSInstall = key.GetValue("InstallDir") as string;
        }

    }
}
