using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

using Microsoft.Win32;

using Cosmos.Build.Installer;

namespace Cosmos.Build.Builder {
  /// <summary>
  /// Cosmos task.
  /// </summary>
  /// <seealso cref="Cosmos.Build.Installer.Task" />
  public class CosmosTask : Task {
    private string mCosmosPath;
    private string mOutputPath;
    private string mAppDataPath;
    private string mInnoFile;
    private string mInnoPath;

    private BuildState mBuildState;
    private int mReleaseNo;
    private List<string> mExceptionList = new List<string>();

    public CosmosTask(string aCosmosDir, int aReleaseNo) {
      mCosmosPath = aCosmosDir;
      mAppDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Cosmos User Kit");
      mReleaseNo = aReleaseNo;
      mInnoFile = Path.Combine(mCosmosPath, @"Setup\Cosmos.iss");
    }

    /// <summary>
    /// Get name of the setup file based on release number and the current setting.
    /// </summary>
    /// <param name="releaseNumber">Release number for the current setup.</param>
    /// <returns>Name of the setup file.</returns>
    public static string GetSetupName(int releaseNumber) {
      string setupName = $"CosmosUserKit-{releaseNumber}-vs2017";

      if (App.UseVsHive) {
        setupName += "Exp";
      }

      return setupName;
    }

    private void CleanDirectory(string aName, string aPath) {
      if (Directory.Exists(mOutputPath)) {
        Section("Cleaning up " + aName + " directory");

        Echo("  " + aPath);
        Directory.Delete(aPath, true);
      }
    }

    protected override List<string> DoRun() {
      mOutputPath = Path.Combine(mCosmosPath, @"Build\VSIP");

      CheckPrereqs();

      if (mBuildState != BuildState.PrerequisiteMissing) {
        CleanDirectory("VSIP", mOutputPath);
        CompileCosmos();
        CreateSetup();

        if (!App.IsUserKit) {
          CleanDirectory("User Kit", mAppDataPath);
          RunSetup();
          WriteDevKit();
          if (!App.DoNotLaunchVS) {
            LaunchVS();
          }
        }

        Done();
      }

      return mExceptionList;
    }

    protected void MSBuild(string aSlnFile, string aBuildCfg) {
      string xMSBuild = Path.Combine(Paths.VSPath, "MSBuild", "15.0", "Bin", "msbuild.exe");
      string xParams = $"{Quoted(aSlnFile)} " +
                       "/nologo " +
                       "/maxcpucount " +
                       "/nodeReuse:False " +
                       $"/p:Configuration={Quoted(aBuildCfg)} " +
                       $"/p:Platform={Quoted("Any CPU")} " +
                       $"/p:OutputPath={Quoted(mOutputPath)}";

      if (!App.NoMSBuildClean) {
        StartConsole(xMSBuild, $"/t:Clean {xParams}");
      }
      StartConsole(xMSBuild, $"/t:Build {xParams}");
    }

    protected int NumProcessesContainingName(string name) {
      return (from x in Process.GetProcesses() where x.ProcessName.Contains(name) select x).Count();
    }

    protected void CheckIfBuilderRunning() {
      //Check for builder process
      Echo("Checking if Builder is already running.");
      // Check > 1 so we exclude ourself.
      if (NumProcessesContainingName("Cosmos.Build.Builder") > 1) {
        throw new Exception("Another instance of builder is running.");
      }
    }

    protected void CheckIfUserKitRunning() {
      Echo("Check if User Kit Installer is already running.");
      if (NumProcessesContainingName("CosmosUserKit") > 0) {
        throw new Exception("Another instance of the user kit installer is running.");
      }
    }

    protected void CheckIfVSRunning() {
      int xSeconds = 500;

      if (Debugger.IsAttached) {
        Echo("Checking if Visual Studio is running is ignored by debugging of Builder.");
      } else {
        Echo("Checking if Visual Studio is running.");
        if (IsRunning("devenv")) {
          Echo("--Visual Studio is running.");
          Echo("--Waiting " + xSeconds + " seconds to see if Visual Studio exits.");
          // VS doesnt exit right away and user can try devkit again after VS window has closed but is still running.
          // So we wait a few seconds first.
          if (WaitForExit("devenv", xSeconds * 1000)) {
            throw new Exception("Visual Studio is running. Please close it or kill it in task manager.");
          }
        }
      }
    }

    protected void NotFound(string aName) {
      mExceptionList.Add("Prerequisite '" + aName + "' not found.");
      mBuildState = BuildState.PrerequisiteMissing;
    }

    protected void CheckPrereqs() {
      Section("Checking Prerequisites");

      CheckIfUserKitRunning();
      CheckIfVSRunning();
      CheckIfBuilderRunning();

      CheckForNetCore();
      CheckForVisualStudioExtensionTools();
      CheckForInno();
    }

    private void CheckForInno() {
      Echo("Checking for Inno Setup");
      using (var xKey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\Inno Setup 5_is1", false)) {
        if (xKey == null) {
          mExceptionList.Add("Cannot find Inno Setup.");
          mBuildState = BuildState.PrerequisiteMissing;
          return;
        }
        mInnoPath = (string)xKey.GetValue("InstallLocation");
        if (string.IsNullOrWhiteSpace(mInnoPath)) {
          mExceptionList.Add("Cannot find Inno Setup.");
          mBuildState = BuildState.PrerequisiteMissing;
          return;
        }
      }

      Echo("Checking for Inno Preprocessor");
      if (!File.Exists(Path.Combine(mInnoPath, "ISPP.dll"))) {
        mExceptionList.Add("Inno Preprocessor not detected.");
        mBuildState = BuildState.PrerequisiteMissing;
        return;
      }
    }

    private void CheckForNetCore() {
      Echo("Checking for .NET Core");

      if (!Paths.VSInstancePackages.Contains("Microsoft.VisualStudio.Workload.NetCoreTools")) {
        mExceptionList.Add(".NET Core not detected.");
        mBuildState = BuildState.PrerequisiteMissing;
      }
    }

    private void CheckForVisualStudioExtensionTools() {
      Echo("Checking for Visual Studio Extension Tools");

      if (!Paths.VSInstancePackages.Contains("Microsoft.VisualStudio.Workload.VisualStudioExtension")) {
        mExceptionList.Add("Visual Studio Extension tools not detected.");
        mBuildState = BuildState.PrerequisiteMissing;
      }
    }

    private void WriteDevKit() {
      Section("Writing Dev Kit to Registry");

      // Inno deletes this from registry, so we must add this after.
      // We let Inno delete it, so if user runs it by itself they get
      // only UserKit, and no DevKit settings.
      // HKCU instead of HKLM because builder does not run as admin.
      //
      // HKCU is not redirected.
      using (var xKey = Registry.CurrentUser.CreateSubKey(@"Software\Cosmos")) {
        xKey.SetValue("DevKit", mCosmosPath);
      }
    }

    private void Restore(string project) {
      string xNuget = Path.Combine(mCosmosPath, "Build", "Tools", "nuget.exe");
      string xRestoreParams = $"restore {Quoted(project)}";
      string xUpdateParams = $"update -self";
      StartConsole(xNuget, xUpdateParams);
      StartConsole(xNuget, xRestoreParams);
    }

    private void Pack(string project, string destDir, string version) {
      string xMSBuild = Path.Combine(Paths.VSPath, "MSBuild", "15.0", "Bin", "msbuild.exe");
      string xParams = $"{Quoted(project)} /nodeReuse:False /t:Restore;Pack /maxcpucount /p:PackageVersion={Quoted(version)} /p:PackageOutputPath={Quoted(destDir)}";
      StartConsole(xMSBuild, xParams);
    }

    private void Publish(string project, string destDir) {
      string xMSBuild = Path.Combine(Paths.VSPath, "MSBuild", "15.0", "Bin", "msbuild.exe");
      string xParams = $"{Quoted(project)} /nodeReuse:False /t:Publish /maxcpucount /p:RuntimeIdentifier=win7-x86 /p:PublishDir={Quoted(destDir)}";
      StartConsole(xMSBuild, xParams);
    }

    private void CompileCosmos() {
      string xVSIPDir = Path.Combine(mCosmosPath, "Build", "VSIP");
      string xPackagesDir = Path.Combine(xVSIPDir, "KernelPackages");
      string xVersion = "1.0.1";

      if (!App.IsUserKit) {
        xVersion += "-" + DateTime.Now.ToString("yyyyMMddHHmm");
      }

      if (!Directory.Exists(xVSIPDir)) {
        Directory.CreateDirectory(xVSIPDir);
      }

      Section("Restoring Nuget Packages");
      Restore(Path.Combine(mCosmosPath, @"Cosmos.sln"));

      Section("Compiling Cosmos");
      MSBuild(Path.Combine(mCosmosPath, @"Build.sln"), "Debug");

      Section("Compiling Tools");
      Publish(Path.Combine(mCosmosPath, "source", "Cosmos.Build.MSBuild"), Path.Combine(xVSIPDir, "MSBuild"));
      Publish(Path.Combine(mCosmosPath, "source", "IL2CPU"), Path.Combine(xVSIPDir, "IL2CPU"));
      Publish(Path.Combine(mCosmosPath, "source", "XSharp.Compiler"), Path.Combine(xVSIPDir, "XSharp"));
      Publish(Path.Combine(mCosmosPath, "Tools", "NASM"), Path.Combine(xVSIPDir, "NASM"));

      Section("Compiling Kernel Packages");
      Pack(Path.Combine(mCosmosPath, "source", "Cosmos.Common"), xPackagesDir, xVersion);
      Pack(Path.Combine(mCosmosPath, "source", "Cosmos.Core"), xPackagesDir, xVersion);
      Pack(Path.Combine(mCosmosPath, "source", "Cosmos.Core.Common"), xPackagesDir, xVersion);
      Pack(Path.Combine(mCosmosPath, "source", "Cosmos.Core.Memory"), xPackagesDir, xVersion);
      Pack(Path.Combine(mCosmosPath, "source", "Cosmos.Core.Plugs"), xPackagesDir, xVersion);
      Pack(Path.Combine(mCosmosPath, "source", "Cosmos.Core.Plugs.Asm"), xPackagesDir, xVersion);
      Pack(Path.Combine(mCosmosPath, "source", "Cosmos.Debug.Kernel"), xPackagesDir, xVersion);
      Pack(Path.Combine(mCosmosPath, "source", "Cosmos.Debug.Kernel.Plugs.Asm"), xPackagesDir, xVersion);
      Pack(Path.Combine(mCosmosPath, "source", "Cosmos.HAL"), xPackagesDir, xVersion);
      Pack(Path.Combine(mCosmosPath, "source", "Cosmos.IL2CPU.Plugs"), xPackagesDir, xVersion);
      Pack(Path.Combine(mCosmosPath, "source", "Cosmos.System"), xPackagesDir, xVersion);
      Pack(Path.Combine(mCosmosPath, "source", "Cosmos.System.Plugs"), xPackagesDir, xVersion);
    }

    private void CopyTemplates() {
      Section("Copying Templates");

      CD(mOutputPath);
      SrcPath = Path.Combine(mCosmosPath, @"source\Cosmos.VS.Package\obj\Debug");
      Copy("CosmosProject (C#).zip", true);
      Copy("CosmosKernel (C#).zip", true);
      Copy("CosmosProject (F#).zip", true);
      Copy("Cosmos.zip", true);
      Copy("CosmosProject (VB).zip", true);
      Copy("CosmosKernel (VB).zip", true);
      Copy(mCosmosPath + @"source\XSharp.VS\Template\XSharpFileItem.zip", true);
    }

    private void CreateSetup() {
      Section("Creating Setup");

      string xISCC = Path.Combine(mInnoPath, "ISCC.exe");
      if (!File.Exists(xISCC)) {
        mExceptionList.Add("Cannot find Inno setup.");
        return;
      }

      string xCfg = App.IsUserKit ? "UserKit" : "DevKit";
      string vsVersionConfiguration = "vs2017";

      // Use configuration which will instal to the VS Exp Hive
      if (App.UseVsHive) {
        vsVersionConfiguration += "Exp";
      }
      Echo($"  {xISCC} /Q {Quoted(mInnoFile)} /dBuildConfiguration={xCfg} /dVSVersion={vsVersionConfiguration} /dVSPath={Quoted(Paths.VSPath)} /dChangeSetVersion={Quoted(mReleaseNo.ToString())}");
      StartConsole(xISCC, $"/Q {Quoted(mInnoFile)} /dBuildConfiguration={xCfg} /dVSVersion={vsVersionConfiguration} /dVSPath={Quoted(Paths.VSPath)} /dChangeSetVersion={Quoted(mReleaseNo.ToString())}");
    }

    private void LaunchVS() {
      Section("Launching Visual Studio");

      string xVisualStudio = Path.Combine(Paths.VSPath, "Common7", "IDE", "devenv.exe");
      if (!File.Exists(xVisualStudio)) {
        mExceptionList.Add("Cannot find Visual Studio.");
        return;
      }

      if (App.ResetHive) {
        Echo("Resetting hive");
        Start(xVisualStudio, @"/setup /rootsuffix Exp /ranu");
      }

      Echo("Launching Visual Studio");
      Start(xVisualStudio, Quoted(mCosmosPath + @"Cosmos.sln"), false, true);
    }

    private void RunSetup() {
      Section("Running Setup");

      KillProcesses("dotnet");
      KillProcesses("msbuild");

      string setupName = GetSetupName(mReleaseNo);

      if (App.UseTask) {
        // This is a hack to avoid the UAC dialog on every run which can be very disturbing if you run
        // the dev kit a lot.
        Start(@"schtasks.exe", @"/run /tn " + Quoted("CosmosSetup"), true, false);

        // Must check for start before stop, else on slow machines we exit quickly because Exit is found before
        // it starts.
        // Some slow user PCs take around 5 seconds to start up the task...
        int xSeconds = 10;
        var xTimed = DateTime.Now;
        Echo("Waiting " + xSeconds + " seconds for Setup to start.");
        if (WaitForStart(setupName, xSeconds * 1000)) {
          mExceptionList.Add("Setup did not start.");
          return;
        }
        Echo("Setup is running. " + DateTime.Now.Subtract(xTimed).ToString(@"ss\.fff"));

        // Scheduler starts it an exits, but we need to wait for the setup itself to exit before proceding
        Echo("Waiting for Setup to complete.");
        WaitForExit(setupName);
      } else {
        Start(mCosmosPath + @"Setup\Output\" + setupName + ".exe", @"/SILENT");
      }
    }

    private void Done() {
      Section("Build Complete!");
    }
  }
}
