using System;
using System.IO;
using Microsoft.Win32;

namespace Cosmos.Build.Common {
  public static class CosmosPaths {
    public static readonly string UserKit;
    public static readonly string Build;
    public static readonly string Vsip;
    public static readonly string Tools;
    public static readonly string IL2CPUTask;
    public static readonly string Kernel;
    public static readonly string GdbClientExe;
    public static readonly string DbgClientExe;
    //
    public static readonly string DevKit = null;
    public static readonly string DebugStubSrc;

    static string CheckPath(string aPath) {
      if (Directory.Exists(aPath) || File.Exists(aPath)) {
        return aPath;
      }
      throw new Exception(aPath + " not found.");
    }

    static CosmosPaths() {
      using (var xReg = Registry.LocalMachine.OpenSubKey(@"Software\Cosmos", false)) {
        if (xReg == null) {
          throw new Exception(@"HKEY_LOCAL_MACHINE\SOFTWARE\Cosmos was not found.");
        }
        UserKit = (string)xReg.GetValue("UserKit");
      }
      Build = CheckPath(Path.Combine(UserKit, @"Build"));
      Vsip = CheckPath(Path.Combine(UserKit, @"Build\VSIP"));
      Tools = CheckPath(Path.Combine(UserKit, @"Build\Tools"));
      IL2CPUTask = CheckPath(Path.Combine(UserKit, @"Build\VSIP\Cosmos.Build.IL2CPUTask.exe"));
      Kernel = CheckPath(Path.Combine(UserKit, @"Kernel"));
      GdbClientExe = CheckPath(Path.Combine(UserKit, @"Build\VSIP\Cosmos.Debug.GDB.exe"));
      DbgClientExe = CheckPath(Path.Combine(UserKit, @"Build\VSIP\Cosmos.VS.Debug.exe"));

      using (var xReg = Registry.CurrentUser.OpenSubKey(@"Software\Cosmos", false)) {
        if (xReg != null) {
          DevKit = (string)xReg.GetValue("DevKit");
          DebugStubSrc = CheckPath(Path.Combine(DevKit, @"source2\Compiler\Cosmos.Compiler.DebugStub\"));
        }
      }
    }
  }
}