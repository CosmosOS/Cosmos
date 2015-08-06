using System.Linq;
using System.Windows;
using Cosmos.Build.Installer;

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
    
    /// <summary>
    /// Version of Visual Studio to use.
    /// </summary>
    public static VsVersion VsVersion;

    /// <summary>
    /// Use Visual Studio Experimental Hive for installing Cosmos Kit.
    /// </summary>
    public static bool UseVsHive;

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
      if (xArgs.Contains("-VS2015") || xArgs.Contains("/VS2015")) {
        VsVersion = VsVersion.Vs2015;
        Paths.VsVersion = VsVersion.Vs2015;
      }

      if (xArgs.Contains("-VSEXPHIVE") || xArgs.Contains("/VSEXPHIVE")) {
        UseVsHive = true;
      }

      base.OnStartup(e);
    }
  }
}
