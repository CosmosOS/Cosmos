using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Win32;
using System.IO;

namespace InstallCleaner
{
    partial class Program
    {
        static void Main(string[] args)
        {
					try{
            CleanupUserKitInstallDir();
            CleanupOldTemplates_Shell_x86();
            CleanupOldTemplates_Shell_x64();
            CleanupOldTemplates_Express();
            } catch(Exception E){
							Console.WriteLine(E.ToString());
							Console.ReadLine();
            }
        }


        /// <summary>
        /// Removes any old cosmos templates in "Microsoft Visual Studio 9.0\Common7\IDE\ProjectTemplates"
        /// </summary>
        private static void CleanupOldTemplates_Shell_x86()
        {
            using (var xReg = Registry.LocalMachine.OpenSubKey(@"Software\Microsoft\VisualStudio\9.0", false))
            {
                if (xReg == null)
                {
                    // shouldn't even happen, but better safe than sorry:
                    return;
                }
                var xInstallDir = xReg.GetValue("InstallDir") as string;
                if (xInstallDir == null)
                {
                    return;
                }
                var xCosmosDir = Path.Combine(xInstallDir, @"ProjectTemplates\Cosmos");

                if (Directory.Exists(xCosmosDir))
                {
                    Directory.Delete(xCosmosDir, true);
                }
            }
        }


        /// <summary>
        /// Removes any old cosmos templates in "Microsoft Visual Studio 9.0\Common7\IDE\ProjectTemplates"
        /// </summary>
        private static void CleanupOldTemplates_Shell_x64() {
            //Registry.LocalMachine.
            using (var xReg = Registry.LocalMachine.OpenSubKey(@"Software\Wow6432\Microsoft\VisualStudio\9.0", false,)) {
                if (xReg == null) {
                    // shouldn't even happen, but better safe than sorry:
                    return;
                }
                var xInstallDir = xReg.GetValue("InstallDir") as string;
                if (xInstallDir == null) {
                    return;
                }
                var xCosmosDir = Path.Combine(xInstallDir, @"ProjectTemplates\Cosmos");

                if (Directory.Exists(xCosmosDir)) {
                    Directory.Delete(xCosmosDir, true);
                }
            }
        }

        /// <summary>
        /// Removes any old cosmos templates in user template directories, most likely in My Documents. Implement for each language.
        /// </summary>
        private static void CleanupOldTemplates_Express()
        {
            using (var xReg = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\VCSExpress\9.0", false))
            {
                if (xReg == null)
                {
                    return;
                }
                var xDir = xReg.GetValue("UserProjectTemplatesLocation") as string;
                if (String.IsNullOrEmpty(xDir))
                {
                    return;
                }
                if (Directory.Exists(xDir))
                {
                    foreach (var xFile in Directory.GetFiles(xDir, "*Cosmos*.zip", SearchOption.AllDirectories))
                    {
                        File.Delete(xFile);
                    }
                }
            }
        }

        /// <summary>
        /// Cleans up the Build and Kernel subdirectories of the installation root.
        /// </summary>
        private static void CleanupUserKitInstallDir()
        {
            using (var xReg = Registry.LocalMachine.OpenSubKey(@"Software\Cosmos", false))
            {
                if (xReg == null)
                {
                    return;
                }
                var xDir = xReg.GetValue(null) as string;
                if (String.IsNullOrEmpty(xDir))
                {
                    return;
                }
                if (Directory.Exists(xDir))
                {
                    if (Directory.Exists(Path.Combine(xDir, "Build")))
                    {
                        Directory.Delete(Path.Combine(xDir, "Build"), true);
                    }
                    if (Directory.Exists(Path.Combine(xDir, "Kernel")))
                    {
                        Directory.Delete(Path.Combine(xDir, "Kernel"), true);
                    }
                }
            }
        }
    }
}
