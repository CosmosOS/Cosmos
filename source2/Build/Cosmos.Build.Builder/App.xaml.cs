using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;

namespace Cosmos.Build.Builder {
  public partial class App : Application {
    public static bool IsUserKit;
    public static bool ResetHive;
    public static bool StayOpen;
    public static bool UseTask;
    public static bool InstallTask;
    public static bool IgnoreVS;

    protected override void OnStartup(StartupEventArgs e) {
      var xArgs = new string[e.Args.Length];
      for (int i = 0; i < xArgs.Length; i++) {
        xArgs[i] = e.Args[i].ToUpper();
      }
      IsUserKit = xArgs.Contains("-USERKIT");
      ResetHive = xArgs.Contains("-RESETHIVE");
      StayOpen = xArgs.Contains("-STAYOPEN");
      UseTask = !xArgs.Contains("-NOTASK");
      InstallTask = xArgs.Contains("-INSTALLTASK");
      IgnoreVS = xArgs.Contains("-IGNOREVS");
      base.OnStartup(e);
    }
  }
}
