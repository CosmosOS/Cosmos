using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Win32;
using System.IO;

namespace InstallCleaner
{
    class Program
    {
        static void Main(string[] args)
        {
            CleanupUserKitInstallDir();
            CleanupOldTemplates_Shell();
            CleanupOldTemplates_Express();
        }


        /// <summary>
        /// Removes any old cosmos templates in "Microsoft Visual Studio 9.0\Common7\IDE\ProjectTemplates"
        /// </summary>
        private static void CleanupOldTemplates_Shell()
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

                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Removes any old cosmos templates in user template directories, most likely in My Documents. Implement for each language.
        /// </summary>
        private static void CleanupOldTemplates_Express()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Cleans up the Build and Kernel subdirectories of the installation root.
        /// </summary>
        private static void CleanupUserKitInstallDir()
        {
            throw new NotImplementedException();
        }
    }
}
