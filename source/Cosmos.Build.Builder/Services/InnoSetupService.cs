using System.IO;
using Microsoft.Win32;

namespace Cosmos.Build.Builder.Services
{
    internal class InnoSetupService : IInnoSetupService
    {
        private const string InnoSetupRegistryKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\Inno Setup 6_is1";

        public string GetInnoSetupInstallationPath()
        {
            using (var localMachineKey32 = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32))
            {
                using (var key = localMachineKey32.OpenSubKey(InnoSetupRegistryKey, false))
                {
                    if (key?.GetValue("InstallLocation") is string innoSetupPath)
                    {
                        if (Directory.Exists(innoSetupPath))
                        {
                            return innoSetupPath;
                        }
                    }
                }
            }

            return null;
        }
    }
}
