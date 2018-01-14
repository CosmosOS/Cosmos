using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Microsoft.Win32;
using NuGet.Configuration;

using Cosmos.Build.Installer;

namespace Cosmos.Build.Builder {
  /// <summary>
  /// Cosmos task.
  /// </summary>
  /// <seealso cref="Cosmos.Build.Installer.Task" />
  public class CosmosTask : Task {
    private string mCosmosPath; // Root Cosmos dir
    private string mVsipPath; // Build/VSIP
    private string mAppDataPath; // User Kit in AppData
    private string mSourcePath; // Cosmos source rood
    private string mInnoPath;
    private string mInnoFile;

    private BuildState mBuildState;
    private int mReleaseNo;
    private List<string> mExceptionList = new List<string>();

    public CosmosTask(string aCosmosDir, int aReleaseNo) {
      mCosmosPath = aCosmosDir;
      mVsipPath = Path.Combine(mCosmosPath, @"Build\VSIP");
      mSourcePath = Path.Combine(mCosmosPath, "source");
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
      if (Directory.Exists(aPath)) {
        Log.WriteLine("Cleaning up existing " + aName + " directory.");
        Directory.Delete(aPath, true);
      }

      Log.WriteLine("Creating " + aName + " as " + aPath);
      Directory.CreateDirectory(aPath);
    }

    protected override List<string> DoRun() {
      if (PrereqsOK()) {
        Section("Init Directories");
        CleanDirectory("VSIP", mVsipPath);
        if (!App.IsUserKit) {
          CleanDirectory("User Kit", mAppDataPath);
        }

        CompileCosmos();
        CreateSetup();

        if (!App.IsUserKit) {
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
                       "/p:DeployExtension=False " +
                       $"/p:Configuration={Quoted(aBuildCfg)} " +
                       $"/p:Platform={Quoted("Any CPU")} " +
                       $"/p:OutputPath={Quoted(mVsipPath)}";

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
      Log.WriteLine("Check if Builder is running.");
      // Check > 1 so we exclude ourself.
      if (NumProcessesContainingName("Cosmos.Build.Builder") > 1) {
        throw new Exception("Another instance of builder is running.");
      }
    }

    protected void CheckIfUserKitRunning() {
      Log.WriteLine("Check if User Kit Installer is already running.");
      if (NumProcessesContainingName("CosmosUserKit") > 0) {
        throw new Exception("Another instance of the user kit installer is running.");
      }
    }

    protected void CheckIfVSandCoRunning() {
      bool xRunningFound = false;
      if (IsRunning("devenv")) {
        xRunningFound = true;
        Log.WriteLine("--Visual Studio is running.");
      }
      if (IsRunning("VSIXInstaller")) {
        xRunningFound = true;
        Log.WriteLine("--VSIXInstaller is running.");
      }
      if (IsRunning("ServiceHub.IdentityHost")) {
        xRunningFound = true;
        Log.WriteLine("--ServiceHub.IdentityHost is running.");
      }
      if (IsRunning("ServiceHub.VSDetouredHost")) {
        xRunningFound = true;
        Log.WriteLine("--ServiceHub.VSDetouredHost is running.");
      }
      if (IsRunning("ServiceHub.Host.Node.x86")) {
        xRunningFound = true;
        Log.WriteLine("--ServiceHub.Host.Node.x86 is running.");
      }
      if (IsRunning("ServiceHub.SettingsHost")) {
        xRunningFound = true;
        Log.WriteLine("--ServiceHub.SettingsHost is running.");
      }
      if (IsRunning("ServiceHub.Host.CLR.x86")) {
        xRunningFound = true;
        Log.WriteLine("--ServiceHub.Host.CLR.x86 is running.");
      }
      if (xRunningFound) {
        Log.WriteLine("--Running blockers found. Setup will warning you and wait for it.");
      }
    }

    protected void NotFound(string aName) {
      mExceptionList.Add("Prerequisite '" + aName + "' not found.");
      mBuildState = BuildState.PrerequisiteMissing;
    }

    protected bool PrereqsOK() {
      Section("Check Prerequisites");

      CheckIfUserKitRunning();
      CheckIfVSandCoRunning();
      CheckIfBuilderRunning();

      CheckForNetCore();
      CheckForVisualStudioExtensionTools();
      CheckForInno();

      return mBuildState != BuildState.PrerequisiteMissing;
    }

    private void CheckForInno() {
      Log.WriteLine("Check for Inno Setup");
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

      Log.WriteLine("Check for Inno Preprocessor");
      if (!File.Exists(Path.Combine(mInnoPath, "ISPP.dll"))) {
        mExceptionList.Add("Inno Preprocessor not detected.");
        mBuildState = BuildState.PrerequisiteMissing;
        return;
      }
    }

    private void CheckForNetCore() {
      Log.WriteLine("Check for .NET Core");

      if (!Paths.VSInstancePackages.Contains("Microsoft.VisualStudio.Workload.NetCoreTools")) {
        mExceptionList.Add(".NET Core not detected.");
        mBuildState = BuildState.PrerequisiteMissing;
      }
    }

    private void CheckForVisualStudioExtensionTools() {
      Log.WriteLine("Check for Visual Studio Extension Tools");

      if (!Paths.VSInstancePackages.Contains("Microsoft.VisualStudio.Workload.VisualStudioExtension")) {
        mExceptionList.Add("Visual Studio Extension tools not detected.");
        mBuildState = BuildState.PrerequisiteMissing;
      }
    }

    private void WriteDevKit() {
      Section("Write Dev Kit to Registry");

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

    private void Clean(string project)
    {
      string xNuget = Path.Combine(mCosmosPath, "Build", "Tools", "nuget.exe");
      string xListParams = $"sources List";
      StartConsole(xNuget, xListParams);

      var xStart = new ProcessStartInfo();
      xStart.FileName = xNuget;
      xStart.WorkingDirectory = CurrPath;
      xStart.Arguments = xListParams;
      xStart.UseShellExecute = false;
      xStart.CreateNoWindow = true;
      xStart.RedirectStandardOutput = true;
      xStart.RedirectStandardError = true;
      using (var xProcess = Process.Start(xStart))
      {
        using (var xReader = xProcess.StandardOutput)
        {
          string xLine;
          while (true)
          {
            xLine = xReader.ReadLine();
            if (xLine == null)
            {
              break;
            }
            if (xLine.Contains("Cosmos Local Package Feed"))
            {
              string xUninstallParams = $"sources Remove -Name \"Cosmos Local Package Feed\"";
              StartConsole(xNuget, xUninstallParams);
            }
          }
        }
      }

      // Clean Cosmos packages from NuGet cache
      var xGlobalFolder = SettingsUtility.GetGlobalPackagesFolder(Settings.LoadDefaultSettings(Environment.SystemDirectory));

      // Later we should specify the packages, currently we're moving to gen3 so package names are a bit unstable
      foreach (var xFolder in Directory.EnumerateDirectories(xGlobalFolder))
      {
        if (new DirectoryInfo(xFolder).Name.StartsWith("Cosmos", StringComparison.InvariantCultureIgnoreCase))
        {
          CleanPackage(xFolder);
        }
      }

      void CleanPackage(string aPackage)
      {
        var xPath = Path.Combine(xGlobalFolder, aPackage);

        if (Directory.Exists(xPath))
        {
          Directory.Delete(xPath, true);
        }
      }
    }

    private void Restore(string project)
    {
      string xNuget = Path.Combine(mCosmosPath, "Build", "Tools", "nuget.exe");
      string xRestoreParams = $"restore {Quoted(project)}";
      StartConsole(xNuget, xRestoreParams);
    }

    private void Update() {
      string xNuget = Path.Combine(mCosmosPath, "Build", "Tools", "nuget.exe");
      string xUpdateParams = $"update -self";
      StartConsole(xNuget, xUpdateParams);
    }
      
    private void Pack(string project, string destDir) {
      string xMSBuild = Path.Combine(Paths.VSPath, "MSBuild", "15.0", "Bin", "msbuild.exe");
      string xParams = $"{Quoted(project)} /nodeReuse:False /t:Restore;Pack /maxcpucount /p:PackageOutputPath={Quoted(destDir)}";
      StartConsole(xMSBuild, xParams);
    }

    private void Publish(string project, string destDir) {
      string xMSBuild = Path.Combine(Paths.VSPath, "MSBuild", "15.0", "Bin", "msbuild.exe");
      string xParams = $"{Quoted(project)} /nodeReuse:False /t:Publish /maxcpucount /p:RuntimeIdentifier=win7-x86 /p:PublishDir={Quoted(destDir)}";
      StartConsole(xMSBuild, xParams);
    }

    private void CompileCosmos() {
      string xVsipDir = Path.Combine(mCosmosPath, "Build", "VSIP");
      string xNugetPkgDir = Path.Combine(xVsipDir, "Packages");

      Section("Clean NuGet Local Feed");
      Clean(Path.Combine(mCosmosPath, @"Build.sln"));

      Section("Restore NuGet Packages");
      Restore(Path.Combine(mCosmosPath, @"Build.sln"));
      Restore(Path.Combine(mCosmosPath, @"../IL2CPU/IL2CPU.sln"));
      Restore(Path.Combine(mCosmosPath, @"../XSharp/XSharp.sln"));

      Section("Update NuGet");
      Update();

      Section("Build Cosmos");
      // Build.sln is the old master but because of how VS manages refs, we have to hack
      // this short term with the new slns.
      MSBuild(Path.Combine(mCosmosPath, @"Build.sln"), "Debug");

      Section("Publish Tools");
      //Publish(Path.Combine(mSourcePath, "Cosmos.Build.MSBuild"), Path.Combine(xVsipDir, "MSBuild"));
      Publish(Path.Combine(mSourcePath, "../../IL2CPU/source/IL2CPU"), Path.Combine(xVsipDir, "IL2CPU"));
      Publish(Path.Combine(mCosmosPath, "Tools", "NASM"), Path.Combine(xVsipDir, "NASM"));

      Section("Create Packages");
      //
      Pack(Path.Combine(mSourcePath, "Cosmos.Build.Tasks"), xNugetPkgDir);
      //
      Pack(Path.Combine(mSourcePath, "Cosmos.Common"), xNugetPkgDir);
      //
      Pack(Path.Combine(mSourcePath, "Cosmos.Core"), xNugetPkgDir);
      Pack(Path.Combine(mSourcePath, "Cosmos.Core_Plugs"), xNugetPkgDir);
      Pack(Path.Combine(mSourcePath, "Cosmos.Core_Asm"), xNugetPkgDir);
      //
      Pack(Path.Combine(mSourcePath, "Cosmos.HAL2"), xNugetPkgDir);
      //
      Pack(Path.Combine(mSourcePath, "Cosmos.System2"), xNugetPkgDir);
      Pack(Path.Combine(mSourcePath, "Cosmos.System2_Plugs"), xNugetPkgDir);
      //
      Pack(Path.Combine(mSourcePath, "Cosmos.Debug.Kernel"), xNugetPkgDir);
      Pack(Path.Combine(mSourcePath, "Cosmos.Debug.Kernel.Plugs.Asm"), xNugetPkgDir);
      //
      Pack(Path.Combine(mSourcePath, "../../IL2CPU/source/IL2CPU.API"), xNugetPkgDir);
    }

    private void CopyTemplates() {
      Section("Copy Templates");

      using (var x = new FileMgr(Path.Combine(mSourcePath, @"Cosmos.VS.Package\obj\Debug"), mVsipPath)) {
        x.Copy("CosmosProject (C#).zip");
        x.Copy("CosmosKernel (C#).zip");
        x.Copy("CosmosProject (F#).zip");
        x.Copy("Cosmos.zip");
        x.Copy("CosmosProject (VB).zip");
        x.Copy("CosmosKernel (VB).zip");
        x.Copy(mSourcePath + @"XSharp.VS\Template\XSharpFileItem.zip");
      }
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

      // Use configuration which will install to the VS Exp Hive
      if (App.UseVsHive) {
        vsVersionConfiguration += "Exp";
      }
      Log.WriteLine($"  {xISCC} /Q {Quoted(mInnoFile)} /dBuildConfiguration={xCfg} /dVSVersion={vsVersionConfiguration} /dChangeSetVersion={Quoted(mReleaseNo.ToString())}");
      StartConsole(xISCC, $"/Q {Quoted(mInnoFile)} /dBuildConfiguration={xCfg} /dVSVersion={vsVersionConfiguration} /dChangeSetVersion={Quoted(mReleaseNo.ToString())}");
    }

    private void LaunchVS() {
      Section("Launching Visual Studio");

      string xVisualStudio = Path.Combine(Paths.VSPath, "Common7", "IDE", "devenv.exe");
      if (!File.Exists(xVisualStudio)) {
        mExceptionList.Add("Cannot find Visual Studio.");
        return;
      }

      if (App.ResetHive) {
        Log.WriteLine("Resetting hive");
        Start(xVisualStudio, @"/setup /rootsuffix Exp /ranu");
      }

      Log.WriteLine("Launching Visual Studio");
      Start(xVisualStudio, Quoted(mCosmosPath + @"Kernel.sln"), false, true);
    }

    private void RunSetup() {
      Section("Running Setup");

      // These cache in RAM which cause problems, so we kill them if present.
      KillProcesses("dotnet");
      KillProcesses("msbuild");

      string setupName = GetSetupName(mReleaseNo);

      Start(mCosmosPath + @"Setup\Output\" + setupName + ".exe", @"/SILENT");
    }

    private void Done() {
      Section("Build Complete!");
    }
  }
}
