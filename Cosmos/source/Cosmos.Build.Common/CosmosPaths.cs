using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Win32;

namespace Cosmos.Build.Common
{
    public static class CosmosPaths
    {
        public static readonly string UserKit;
        public static readonly string Build;
        public static readonly string Vsip;
        public static readonly string Tools;
        public static readonly string Kernel;
        public static readonly string GdbClientExe;
        //
        public static readonly string DevKit = null;
        public static string DebugStubSrc;

        static string CheckPath(string aPath1, string aPath2)
        {
            return CheckPath(Path.Combine(aPath1, aPath2));
        }

        static string CheckPath(string aPath)
        {
            if (Directory.Exists(aPath) || File.Exists(aPath))
            {
                return aPath;
            }
            throw new Exception(aPath + " not found.");
        }

        static CosmosPaths()
        {
            using (RegistryKey baseKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32))
            {
                using (var xReg = baseKey.OpenSubKey(@"Software\Cosmos", false))
                {
                    if (xReg == null)
                    {
                        throw new Exception(@"HKEY_LOCAL_MACHINE\SOFTWARE\Cosmos was not found.");
                    }
                    UserKit = (string) xReg.GetValue("UserKit");
                    if (null == UserKit)
                    {
                        throw new Exception(@"HKEY_LOCAL_MACHINE\SOFTWARE\Cosmos\@UserKit was not found but UserKit must be installed!");
                    }
                }
            }
            Build = CheckPath(UserKit, @"Build");
            Vsip = CheckPath(UserKit, @"Build\VSIP");
            Tools = CheckPath(UserKit, @"Build\Tools");
            Kernel = CheckPath(UserKit, @"Kernel");
            GdbClientExe = CheckPath(UserKit, @"Build\VSIP\Cosmos.Debug.GDB.exe");
            DebugStubSrc = CheckPath(UserKit, @"XSharp\DebugStub");

            // Not finding this ones is not an issue. We will fallback to already retrieved stub from UserKit
            using (var xReg = Registry.CurrentUser.OpenSubKey(@"Software\Cosmos", false))
            {
                if (xReg != null)
                {
                    DevKit = (string) xReg.GetValue("DevKit");
                    try
                    {
                        DebugStubSrc = CheckPath(DevKit, @"source\Cosmos.Core.DebugStub");
                    }
                    catch
                    {
                    }
                }
            }
        }

        /// <summary>This method is intentionally empty. It's sole purpose is to be able to trigger
        /// class initialization in a controlled manner so as to intercept initializer thrown
        /// exceptions.</summary>
        public static void Initialize()
        {
        }
    }
}
