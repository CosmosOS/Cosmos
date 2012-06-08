using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Cosmos.Build.Installer {
  public static class Paths {
    static Paths() {
      if (Global.IsX64) {
        ProgFiles32 = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
        ProgFiles64 = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
      } else {
        ProgFiles32 = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
      }

      Windows = Environment.GetFolderPath(Environment.SpecialFolder.Windows);
    }

    public static readonly string ProgFiles32;
    public static readonly string ProgFiles64;

    public static readonly string Windows;

    public static string Current {
      get { return Directory.GetCurrentDirectory(); }
      set { Directory.SetCurrentDirectory(value); }
    }
  }
}
