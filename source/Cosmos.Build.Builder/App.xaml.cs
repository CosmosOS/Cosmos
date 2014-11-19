using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;

namespace Cosmos.Build.Builder {
  public partial class App : Application {
    public static bool DoNotLaunchVS;
    public static bool IsUserKit;
    public static bool ResetHive;
    public static bool StayOpen;
    public static bool UseTask;
    public static bool NoMsBuildClean;
    public static bool InstallTask;
    public static bool IgnoreVS;
    public static bool TestMode = false;
    public static bool HasParams = false;

    protected override void OnStartup(StartupEventArgs e) {
      HasParams = e.Args.Length > 0;

      var xArgs = new string[e.Args.Length];
      for (int i = 0; i < xArgs.Length; i++) {
        xArgs[i] = e.Args[i].ToUpper();
      }
      IsUserKit = xArgs.Contains("-USERKIT");
      ResetHive = xArgs.Contains("-RESETHIVE");
      StayOpen = xArgs.Contains("-STAYOPEN");
      UseTask = !xArgs.Contains("-NOTASK");
      NoMsBuildClean = xArgs.Contains("-NOCLEAN");
      InstallTask = xArgs.Contains("-INSTALLTASK");
      DoNotLaunchVS = xArgs.Contains("-NOVSLAUNCH");
      // For use during dev of Builder only.
      IgnoreVS = xArgs.Contains("-IGNOREVS");
      TestMode = xArgs.Contains("-TESTMODE");
      base.OnStartup(e);
    }
  }
}
