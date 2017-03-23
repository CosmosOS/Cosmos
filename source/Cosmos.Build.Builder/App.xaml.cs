using System;
using System.Linq;
using System.Windows;

using Cosmos.Build.Installer;

namespace Cosmos.Build.Builder
{
  public partial class App : Application
  {
    public static bool DoNotLaunchVS;
    public static bool IsUserKit;
    public static bool ResetHive;
    public static bool StayOpen;
    public static bool UseTask;
    public static bool NoMSBuildClean;
    public static bool InstallTask;
    public static bool IgnoreVS;
    public static bool TestMode = false;
    public static bool HasParams = false;

    /// <summary>
    /// Version of Visual Studio to use.
    /// </summary>
    public static VSVersion VSVersion;

    /// <summary>
    /// Use Visual Studio Experimental Hive for installing Cosmos Kit.
    /// </summary>
    public static bool UseVsHive;

    protected override void OnStartup(StartupEventArgs e)
    {
      HasParams = e.Args.Length > 0;

      var xArgs = new string[e.Args.Length];
      for (int i = 0; i < xArgs.Length; i++)
      {
        xArgs[i] = e.Args[i].ToUpper();
      }
      IsUserKit = xArgs.Contains("-USERKIT");
      ResetHive = xArgs.Contains("-RESETHIVE");
      StayOpen = xArgs.Contains("-STAYOPEN");
      UseTask = !xArgs.Contains("-NOTASK");
      NoMSBuildClean = xArgs.Contains("-NOCLEAN");
      InstallTask = xArgs.Contains("-INSTALLTASK");
      DoNotLaunchVS = xArgs.Contains("-NOVSLAUNCH");
      // For use during dev of Builder only.
      IgnoreVS = xArgs.Contains("-IGNOREVS");
      TestMode = xArgs.Contains("-TESTMODE");

      if (xArgs.Contains("-VS2017") || xArgs.Contains("/VS2017"))
      {
        VSVersion = VSVersion.VS2017;
      }

      Paths.VSVersion = VSVersion;

      Paths.VSInstanceID = xArgs.Where(arg => arg.StartsWith("-VSINSTANCE=") || arg.StartsWith("/VSINSTANCE=")).SingleOrDefault()?.Substring(12).ToLowerInvariant();

      UseVsHive = xArgs.Contains("-VSEXPHIVE") || xArgs.Contains("/VSEXPHIVE");

      base.OnStartup(e);
    }
  }
}
