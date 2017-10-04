using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

using Cosmos.Build.Installer;

namespace Cosmos.Build.Builder {
  public partial class App : Application {
    public static bool DoNotLaunchVS;
    public static bool IsUserKit;
    public static bool ResetHive;
    public static bool StayOpen;
    public static bool NoMSBuildClean;
    public static bool UseVsHive;
    public static Dictionary<string, string> mArgs = new Dictionary<string, string>();

    protected override void OnStartup(StartupEventArgs e) {
      foreach (string arg in e.Args) {
        string[] keyValue = arg.Split('=');
        if (keyValue.Length > 0) {
          string key = keyValue[0].ToUpper().Remove(0, 1);
          mArgs.Add(key, "");
          if (keyValue.Length > 1) {
            mArgs[key] = keyValue[1];
          }
        }
      }

      IsUserKit = mArgs.ContainsKey("USERKIT");
      ResetHive = mArgs.ContainsKey("RESETHIVE");
      StayOpen = mArgs.ContainsKey("STAYOPEN");
      NoMSBuildClean = mArgs.ContainsKey("NOCLEAN");
      DoNotLaunchVS = mArgs.ContainsKey("NOVSLAUNCH");
      UseVsHive = mArgs.ContainsKey("VSEXPHIVE");

      if (mArgs.ContainsKey("VSPATH")) {
        Paths.VSPath = mArgs["VSPATH"];
        Paths.UpdateVSPath();
      } else {
        throw new ArgumentNullException(nameof(e.Args), "Visual Studio path must be provided. (-VSPATH or /VSPATH)");
      }

      // For debugging, set params to something like this:
      // -VSPath=C:\Program Files (x86)\Microsoft Visual Studio\2017\Enterprise

      base.OnStartup(e);
    }
  }
}
