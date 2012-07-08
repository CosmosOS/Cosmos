using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cosmos.Build.Installer;
using System.IO;
using Microsoft.Win32;
using System.Windows;
using TaskScheduler;

namespace Cosmos.Build.Builder {
  public class CosmosTask : Task {
    protected string mCosmosPath;
    public bool ResetHive { get; set; }
    protected string mOutputPath;
    public bool IsUserKit { get; set; }
    protected int mReleaseNo;
    protected string mInnoFile;

    public CosmosTask(string aCosmosPath, int aReleaseNo) {
      mCosmosPath = aCosmosPath;
      mReleaseNo = aReleaseNo;
      mInnoFile = mCosmosPath + @"\Setup2\Cosmos.iss";
    }

    protected void MsBuild(string aSlnFile, string aBuildCfg) {
      StartConsole(Paths.Windows + @"\Microsoft.NET\Framework\v4.0.30319\msbuild.exe", Quoted(aSlnFile) + @" /maxcpucount /verbosity:normal /nologo /p:Configuration=" + aBuildCfg + " /p:Platform=x86 /p:OutputPath=" + Quoted(mOutputPath));
    }

    protected bool CheckForInstall(string aCheck, bool aCanThrow) {
      return CheckForProduct(aCheck, aCanThrow, @"SOFTWARE\Classes\Installer\Products\", "ProductName");
    }
    protected bool CheckForUninstall(string aCheck, bool aCanThrow) {
      return CheckForProduct(aCheck, aCanThrow, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\", "DisplayName");
    }
    protected bool CheckForProduct(string aCheck, bool aCanThrow, string aKey, string aValueName) {
      string xCheck = aCheck.ToUpper();
      string[] xKeys;
      using (var xKey = Registry.LocalMachine.OpenSubKey(aKey, false)) {
        xKeys = xKey.GetSubKeyNames();
      }
      foreach (string xSubKey in xKeys) {
        using (var xKey = Registry.LocalMachine.OpenSubKey(aKey + xSubKey, false)) {
          string xValue = (string)xKey.GetValue(aValueName);
          if (xValue != null && xValue.ToUpper().Contains(xCheck)) {
            return true;
          }
        }
      }

      if (aCanThrow) {
        NotFound(aCheck);
      }
      return false;
    }

    protected void CheckNet35Sp1() {
      bool xNet35SP1Installed = false;
      using (var xKey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\NET Framework Setup\NDP\v3.5", false)) {
        if (xKey != null) {
          xNet35SP1Installed = (int)xKey.GetValue("SP", 0) >= 1;
        }
      }
      if (!xNet35SP1Installed) {
        NotFound(".NET 3.5 SP1");
      }
    }

    protected void NotFound(string aName) {
      throw new Exception("Prerequisite '" + aName + "' not found.");
    }

    protected void CheckPrereqs() {
      Section("Checking Prerequisites");
      Echo("Note: This does not check all prerequisites, please see website for full list.");

      // We assume they have normal .NET stuff if user was able to build the builder...
      //Visual Studio 2010

      CheckNet35Sp1(); // Required by VMWareLib
      CheckForUninstall("Inno Setup QuickStart Pack", true);
      CheckForInstall("Microsoft Visual Studio 2010 SDK SP1", true);
      if (!CheckForInstall("VMware Workstation", false)) {
        if (!CheckForInstall("VMware Player", false)) {
          NotFound("VMWare");
        }
      }
      CheckForInstall("VMWare VIX", true);
    }

    protected override void DoRun() {
      mOutputPath = Path.Combine(mCosmosPath, @"Build\VSIP");
      if (!Directory.Exists(mOutputPath)) {
        Directory.CreateDirectory(mOutputPath);
      }

      CheckScheduledTask();
      return;

      CheckPrereqs();
      CompileXSharpCompiler();
      CompileXSharpSource();
      CompileCosmos();
      CopyTemplates();
      if (IsUserKit) {
        CreateUserKitScript();
      }
      CreateSetup();
      if (!IsUserKit) {
        RunSetup();
        LaunchVS();
      }

      Done();
    }

    void CreateUserKitScript() {
      Section("Creating User Kit Script");

      // Read in Cosmos.iss
      using (var xSrc = new StreamReader(mInnoFile)) {
        mInnoFile = Path.Combine(Path.GetDirectoryName(mInnoFile), "UserKit.iss");
        // Write out UserKit.iss
        using (var xDest = new StreamWriter(mInnoFile)) {
          string xLine;
          while ((xLine = xSrc.ReadLine()) != null) {
            if (xLine.StartsWith("#define ChangeSetVersion ", StringComparison.InvariantCultureIgnoreCase)) {
              xDest.WriteLine("#define ChangeSetVersion " + Quoted(mReleaseNo.ToString()));
            } else {
              xDest.WriteLine(xLine);
            }
          }
        }
      }
    }

    void CompileXSharpCompiler() {
      Section("Compiling X# Compiler");

      MsBuild(mCosmosPath + @"source2\XSharp.sln", "Debug");
    }

    void CompileXSharpSource() {
      Section("Compiling X# Sources");
      
      var xFiles = Directory.GetFiles(mCosmosPath + @"source2\Compiler\Cosmos.Compiler.DebugStub\", "*.xs");
      foreach (var xFile in xFiles) {
        Echo("Compiling " + Path.GetFileName(xFile));
        string xDest = Path.ChangeExtension(xFile, ".cs");
        if (File.Exists(xDest)) {
          ResetReadOnly(xDest);
        }
        // We dont ref the X# asm directly because then we could not compile it without dynamic loading.
        // This way we can build it and call it directly.
        StartConsole(mOutputPath + @"\xsc.exe", Quoted(xFile) + @" Cosmos.Debug.DebugStub");
      }
    }

    void CompileCosmos() {
      Section("Compiling Cosmos");

      MsBuild(mCosmosPath + @"\source\Build.sln", "Debug");
    }

    void CopyTemplates() {
      Section("Copying Templates");

      CD(mOutputPath);
      SrcPath = Path.Combine(mCosmosPath, @"source2\VSIP\Cosmos.VS.Package\obj\x86\Debug");
      Copy("CosmosProject (C#).zip");
      Copy("CosmosKernel (C#).zip");
      Copy("CosmosProject (F#).zip");
      Copy("Cosmos.zip");
      Copy("CosmosProject (VB).zip");
      Copy("CosmosKernel (VB).zip");
      Copy(mCosmosPath + @"source2\VSIP\Cosmos.VS.XSharp\Template\XSharpFileItem.zip");
    }

    void CreateSetup() {
      Section("Creating Setup");

      string xInnoPath;
      using (var xKey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\Inno Setup 5_is1", false)) {
        if (xKey == null) {
          throw new Exception("Cannot find Inno Setup.");
        }
        xInnoPath = (string)xKey.GetValue("InstallLocation");
        if (string.IsNullOrWhiteSpace(xInnoPath)) {
          throw new Exception("Cannot find Inno Setup.");
        }
      }

      string xISCC = Path.Combine(xInnoPath, "ISCC.exe");
      if (!File.Exists(xISCC)) {
        throw new Exception("Cannot find Inno setup.");
      }
      string xCfg = IsUserKit ? "UserKit" : "DevKit";
      StartConsole(xISCC, @"/Q " + Quoted(mInnoFile) + " /dBuildConfiguration=" + xCfg);

      if (IsUserKit) {
        File.Delete(mInnoFile);
      }
    }

    void LaunchVS() {
      Section("Launching Visual Studio");
     
      string xVisualStudio = Paths.ProgFiles32 + @"\Microsoft Visual Studio 10.0\Common7\IDE\devenv.exe";
      if (!File.Exists(xVisualStudio)) {
        throw new Exception("Cannot find Visual Studio.");
      }

      if (ResetHive) {
        Echo("Resetting hive");
        Start(xVisualStudio, @"/setup /rootsuffix Exp /ranu");
      }

      Echo("Launching Visual Studio");
      Start(xVisualStudio, mCosmosPath + @"\source\Cosmos.sln", false);
    }

    bool mTaskIsInstalled = false;
    void CheckScheduledTask() {
      ITaskService xService = new TaskScheduler.TaskScheduler();
      xService.Connect();
      var xTasks = new List<IRegisteredTask>();
      ITaskFolder xFolder = xService.GetFolder(@"\");
      foreach (IRegisteredTask xTask in xFolder.GetTasks(0)) {
        if (string.Equals(xTask.Name, "CosmosSetup")) {
          mTaskIsInstalled = true;
          break;
        }
      }
    }

    void RunSetup() {
      Section("Running Setup");

      // This is a hack to avoid the UAC dialog on every run which can be very disturbing if you run
      // the dev kit a lot.
      CheckScheduledTask();
      if (mTaskIsInstalled) {
        Start(@"schtasks.exe", @"/run /tn " + Quoted("CosmosSetup"));
      } else {
        Start(mCosmosPath + @"\Setup2\Output\CosmosUserKit-" + mReleaseNo + ".exe", @"/SILENT");
      }
    }

    void Done() {
      Section("Build Complete!");
    }
  }
}
