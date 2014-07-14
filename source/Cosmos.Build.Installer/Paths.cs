using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Microsoft.Win32;
namespace Cosmos.Build.Installer {
  public static class Paths {
    static Paths() {
      if (Global.IsX64) {
        ProgFiles32 = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
        ProgFiles64 = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
      } else {
        ProgFiles32 = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
      }
			// The Install Dir will pickup only the Visual Studio 2013 path currently.
			RegistryKey key = Registry.LocalMachine.OpenSubKey(string.Format(@"SOFTWARE{0}\microsoft\VisualStudio\12.0",
			 Environment.Is64BitOperatingSystem ? @"\Wow6432Node" : ""));
			VSInstall = key.GetValue("InstallDir") as string;
      Windows = Environment.GetFolderPath(Environment.SpecialFolder.Windows);
    }
    public static readonly string ProgFiles32;
    public static readonly string ProgFiles64;
		public static readonly string VSInstall;
    public static readonly string Windows;
  }
}
