using System;
using Registry = Microsoft.Win32.Registry;
using Path = System.IO.Path;

namespace Cosmos.Build.Common {
  public static class CosmosPaths {
    public static readonly string CosmosKit;
    public static readonly string Build;
    public static readonly string BuildVsip;
    public static readonly string Tools;
    public static readonly string IL2CPUTask;
    public static readonly string Kernel;
    public static readonly string GDBClientExe;
    public static readonly string DBGClientExe;

    static CosmosPaths() {
      using (var xReg = Registry.LocalMachine.OpenSubKey("Software\\Cosmos", false)) {
        if (xReg == null) {
          throw new Exception("The Key \"HKEY_LOCAL_MACHINE\\SOFTWARE\\Cosmos\" not found.");
        }
        CosmosKit = (string)xReg.GetValue(null);
      }

      Build = Path.Combine(CosmosKit, "Build");
      BuildVsip = Path.Combine(CosmosKit, "Build\\VSIP");
      BuildVsip = Path.Combine(CosmosKit, "Build\\Tools");
      IL2CPUTask = Path.Combine(CosmosKit, "Build\\VSIP\\Cosmos.Build.IL2CPUTask.exe");
      Kernel = Path.Combine(CosmosKit, "Kernel");
      GDBClientExe = Path.Combine(CosmosKit, "Build\\VSIP\\Cosmos.Debug.GDB.exe");
      DBGClientExe = Path.Combine(CosmosKit, "Build\\VSIP\\Cosmos.VS.Debug.exe");
    }
  }
}