using System;
using Registry = Microsoft.Win32.Registry;
using Path = System.IO.Path;

namespace Cosmos.Build.Common {
  public static class CosmosPaths {
    public static readonly string UserKit;
    public static readonly string DevKit = null;
    public static readonly string Build;
    public static readonly string Vsip;
    public static readonly string Tools;
    public static readonly string IL2CPUTask;
    public static readonly string Kernel;
    public static readonly string GdbClientExe;
    public static readonly string DbgClientExe;

    static CosmosPaths() {
      using (var xReg = Registry.LocalMachine.OpenSubKey(@"Software\Cosmos", false)) {
        if (xReg == null) {
          throw new Exception(@"HKEY_LOCAL_MACHINE\SOFTWARE\Cosmos was not found.");
        }
        UserKit = (string)xReg.GetValue("UserKit");
      }
      Build = Path.Combine(UserKit, @"Build");
      Vsip = Path.Combine(UserKit, @"Build\VSIP");
      Tools = Path.Combine(UserKit, @"Build\Tools");
      IL2CPUTask = Path.Combine(UserKit, @"Build\VSIP\Cosmos.Build.IL2CPUTask.exe");
      Kernel = Path.Combine(UserKit, @"Kernel");
      GdbClientExe = Path.Combine(UserKit, @"Build\VSIP\Cosmos.Debug.GDB.exe");
      DbgClientExe = Path.Combine(UserKit, @"Build\VSIP\Cosmos.VS.Debug.exe");

      using (var xReg = Registry.CurrentUser.OpenSubKey(@"Software\Cosmos", false)) {
        if (xReg != null) {
          DevKit = (string)xReg.GetValue("DevKit");
        }
      }
    }
  }
}