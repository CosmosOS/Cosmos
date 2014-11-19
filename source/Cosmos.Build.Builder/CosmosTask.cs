using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Cosmos.Build.Installer;
using System.IO;
using Microsoft.Win32;
using System.Windows;

namespace Cosmos.Build.Builder {
  public class CosmosTask : Task {
    protected string mCosmosDir;
    protected string mOutputDir;
    protected string mAppDataDir;
    protected int mReleaseNo;
    protected string mInnoFile;
    protected string mInnoPath;
    public string InnoScriptTargetFile = "Current.iss";

    public CosmosTask(string aCosmosDir, int aReleaseNo) {
      mCosmosDir = aCosmosDir;
      mAppDataDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Cosmos User Kit");
      mReleaseNo = aReleaseNo;
      mInnoFile = Path.Combine(mCosmosDir, @"Setup\Cosmos.iss");
    }

    void CleanupVSIPFolder() {
      if (Directory.Exists(mOutputDir)) {
        Section("Cleaning up VSIP Folder");

        // Make sure no files are left, else things can be not be rebuilt and when adding
        // new items this can cause issues.
        Echo("Deleting build output directory.");
        Echo("  " + mOutputDir);
        Directory.Delete(mOutputDir, true);
      }
    }

    void CleanupAlreadyInstalled() {
      //in case install folder is the same like the last installation, inno setup delete already this path!
      // mean this is normally not needed, what do you think?
      if (Directory.Exists(mAppDataDir)) {
        Section("Cleaning up currently installed user kit directory");
        Echo("  " + mAppDataDir);
        Directory.Delete(mAppDataDir, true);
      }
    }

    protected override void DoRun() {
      mOutputDir = Path.Combine(mCosmosDir, @"Build\VSIP");

      if (!App.TestMode) {
        CheckPrereqs();                                 //Working
        CleanupVSIPFolder();

        CompileCosmos();                                //Working
        CopyTemplates();
        
        CreateScriptToUseChangesetWhichTaskIsUse();

        CreateSetup();                                  //Working
        if (!App.IsUserKit) {
          CleanupAlreadyInstalled();                      //Working
          RunSetup();                                 //Working - forgot to run as user kit first
          WriteDevKit();                              //Working
          if (!App.DoNotLaunchVS) { LaunchVS(); }     //Working
        }

        Done();
      } else {
        Section("Testing...");
        //Uncomment bits that you want to test...
        CheckForInno(); //CheckPrereqs();                               //Working
        //Cleanup();                                    //Working

        //CompileCosmos();                              //Working
        //CopyTemplates();                              //Working
        //if (App.IsUserKit)
        //{
        //    CreateUserKitScript();                    //Working
        //}
        //CreateSetup();                                //Working
        //if (!App.IsUserKit)
        //{
        //    RunSetup();                               //Working
        //    WriteDevKit();                            //Working
        //    if (!App.DoNotLaunchVS) { LaunchVS(); }   //Working
        //}

        //Done();
      }
    }

    protected void MsBuild(string aSlnFile, string aBuildCfg) {
      string xMsBuild = Path.Combine(Paths.Windows, @"Microsoft.NET\Framework\v4.0.30319\msbuild.exe");
      string xParams = Quoted(aSlnFile) + @" /maxcpucount /verbosity:normal /nologo /p:Configuration=" + aBuildCfg + " /p:Platform=x86 /p:OutputPath=" + Quoted(mOutputDir);
      // Clean then build: http://adrianfoyn.wordpress.com/2011/03/30/wrestling-with-msbuild-the-bane-of-trebuild/
      if (false == App.NoMsBuildClean) {
        StartConsole(xMsBuild, "/t:Clean " + xParams);
      }
      StartConsole(xMsBuild, "/t:Build " + xParams);
    }

    protected int NumProcessesContainingName(string name) {
      return (from x in Process.GetProcesses() where x.ProcessName.Contains(name) select x).Count();
    }

    protected bool CheckForInstall(string aCheck, bool aCanThrow) {
      return CheckForProduct(aCheck, aCanThrow, @"SOFTWARE\Classes\Installer\Products\", "ProductName");
    }
    protected bool CheckForUninstall(string aCheck, bool aCanThrow) {
      return CheckForProduct(aCheck, aCanThrow, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\", "DisplayName");
    }
    protected bool CheckForProduct(string aCheck, bool aCanThrow, string aKey, string aValueName) {
      Echo("Checking for " + aCheck);
      string xCheck = aCheck.ToUpper();
      string[] xKeys;
      using (var xKey = Registry.LocalMachine.OpenSubKey(aKey, false)) {
        xKeys = xKey.GetSubKeyNames();
      }
      foreach (string xSubKey in xKeys) {
        using (var xKey = Registry.LocalMachine.OpenSubKey(aKey + xSubKey)) {
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
      Echo("Checking for .NET 3.5 SP1");
      bool xInstalled = false;
      using (var xKey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\NET Framework Setup\NDP\v3.5")) {
        if (xKey != null) {
          xInstalled = (int)xKey.GetValue("SP", 0) >= 1;
        }
      }
      if (!xInstalled) {
        NotFound(".NET 3.5 SP1");
      }
    }

    protected void CheckNet403() {
      Echo("Checking for .NET 4.03");
      if (Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\.NETFramework\v4.0.30319\SKUs\.NETFramework,Version=v4.0.3") == null) {
        NotFound(".NET 4.03 Full Install (not client)");
      }
    }

    protected void CheckOS() {
      Echo("Checking Operating System");
      var xOsInfo = System.Environment.OSVersion;
      if (xOsInfo.Platform != PlatformID.Win32NT) {
        NotFound("Supported OS");
      }
      decimal xVer = decimal.Parse(xOsInfo.Version.Major + "." + xOsInfo.Version.Minor, System.Globalization.CultureInfo.InvariantCulture);
      // 6.0 Vista
      // 6.1 2008
      // 6.2 Windows 7
      // 6.3 Windows 8
      // 6.4 Windows 10
      if (xVer < 6.0m) {
        NotFound("Minimum Supported OS is Vista/2008");
      }
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
      if (NumProcessesContainingName("CosmosUserKit") > 1) {
        throw new Exception("Another instance of the user kit installer is running.");
      }
    }

    protected void CheckIsVsRunning() {
      int xSeconds = 500;
      if (App.IgnoreVS) {
        return;
      }

      if (AreWeNowDebugTheBuilder()) {
        Echo("Checking if Visual Studio is running is ignored by debugging of Builder.");
      }
      else {
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

    private bool AreWeNowDebugTheBuilder() {
        return Process.GetCurrentProcess().ProcessName.EndsWith(".vshost");
    }

    protected void NotFound(string aName) {
      throw new Exception("Prerequisite '" + aName + "' not found.");
    }

    protected void CheckPrereqs() {
      Section("Checking Prerequisites");
      Echo("Note: This check only prerequisites for building, please see website for full list.");

      Echo("Checking for x86 run.");
      if (!AmRunning32Bit()) {
        throw new Exception("Builder must run as x86");
      }

      // We assume they have normal .NET stuff if user was able to build the builder...

      CheckOS();
      CheckIfUserKitRunning();
      CheckIsVsRunning();
      CheckIfBuilderRunning();

      CheckVs2013();

      //works also without, only close of VMWare is not working! CheckNet35Sp1(); // Required by VMWareLib
      CheckNet403();
      CheckForInno();
      CheckForInstall("Microsoft Visual Studio 2013 SDK", true);
      bool vmWareInstalled = true;
      bool bochsInstalled = IsBochsInstalled();
      if (!CheckForInstall("VMware Workstation", false)) {
        if (!CheckForInstall("VMware Player", false)) {
          // Fix issue #15553
          if (!CheckForInstall("VMwarePlayer_x64", false)) {
            vmWareInstalled = false;
          }
        }
      }
      if (!vmWareInstalled && !bochsInstalled) { NotFound("VMWare or Bochs"); }

      // VIX is installed with newer VMware Workstations (8+ for sure). VMware player does not install it?
      // We need to just watch this and adjust as needed.
      //CheckForInstall("VMWare VIX", true);
    }

    /// <summary>Check for Bochs being installed.</summary>
    private static bool IsBochsInstalled() {
      try {
        using (var runCommandRegistryKey = Registry.ClassesRoot.OpenSubKey(@"BochsConfigFile\shell\Run\command", false)) {
          if (null == runCommandRegistryKey) { return false; }
          string commandLine = (string)runCommandRegistryKey.GetValue(null, null);
          if (null != commandLine) { commandLine = commandLine.Trim(); }
          if (string.IsNullOrEmpty(commandLine)) { return false; }
          // Now perform some parsing on command line to discover full exe path.
          string candidateFilePath;
          int commandLineLength = commandLine.Length;
          if ('"' == commandLine[0]) {
            // Seek for a non escaped double quote.
            int lastDoubleQuoteIndex = 1;
            for (; lastDoubleQuoteIndex < commandLineLength; lastDoubleQuoteIndex++) {
              if ('"' != commandLine[lastDoubleQuoteIndex]) { continue; }
              if ('\\' != commandLine[lastDoubleQuoteIndex - 1]) { break; }
            }
            if (lastDoubleQuoteIndex >= commandLineLength) { return false; }
            candidateFilePath = commandLine.Substring(1, lastDoubleQuoteIndex - 1);
          } else {
            // Seek for first separator character.
            int firstSeparatorIndex = 0;
            for (; firstSeparatorIndex < commandLineLength; firstSeparatorIndex++) {
              if (char.IsSeparator(commandLine[firstSeparatorIndex])) { break; }
            }
            if (firstSeparatorIndex >= commandLineLength) { return false; }
            candidateFilePath = commandLine.Substring(0, firstSeparatorIndex);
          }
          return File.Exists(candidateFilePath);
        }
      } catch { return false; }
    }

    void CheckForInno() {
      Echo("Checking for Inno Setup");
      using (var xKey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\Inno Setup 5_is1", false)) {
        if (xKey == null) {
          throw new Exception("Cannot find Inno Setup.");
        }
        mInnoPath = (string)xKey.GetValue("InstallLocation");
        if (string.IsNullOrWhiteSpace(mInnoPath)) {
          throw new Exception("Cannot find Inno Setup.");
        }
      }

      Echo("Checking for Inno Preprocessor");
      if (!File.Exists(Path.Combine(mInnoPath, "ISPP.dll"))) {
        throw new Exception("Inno Preprocessor not detected.");
      }
    }

    void CheckVs2013() {
      Echo("Checking for Visual Studio 2013");
      string key = @"SOFTWARE\Microsoft\VisualStudio\12.0";
      if (Environment.Is64BitOperatingSystem)
        key = @"SOFTWARE\Wow6432Node\Microsoft\VisualStudio\12.0";
      using (var xKey = Registry.LocalMachine.OpenSubKey(key)) {
        string xDir = (string)xKey.GetValue("InstallDir");
        if (String.IsNullOrWhiteSpace(xDir)) {
          throw new Exception("Visual Studio 2013 not detected!");
        }
      }
    }

    void WriteDevKit() {
      Section("Writing Dev Kit to Registry");

      // Inno deletes this from registry, so we must add this after. 
      // We let Inno delete it, so if user runs it by itself they get
      // only UserKit, and no DevKit settings.
      // HKCU instead of HKLM because builder does not run as admin.
      //
      // HKCU is not redirected.
      using (var xKey = Registry.CurrentUser.CreateSubKey(@"Software\Cosmos")) {
        xKey.SetValue("DevKit", mCosmosDir);
      }
    }

    void CreateScriptToUseChangesetWhichTaskIsUse() {
      Section("Creating Inno Setup Script");

      // Read in iss file
      using (var xSrc = new StreamReader(mInnoFile)) {
        mInnoFile = Path.Combine(Path.GetDirectoryName(mInnoFile), InnoScriptTargetFile);
        // Write out new iss
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
      MsBuild(Path.Combine(mCosmosDir, @"source\XSharp.sln"), "Debug");
    }

    void CompileXSharpSource() {
      Section("Compiling X# Sources");

      // XSC can do all files in path, but we do it on our own currently for better status updates.
      // When we get xsproj files we can build directly.
      var xFiles = Directory.GetFiles(mCosmosDir + @"source2\Compiler\Cosmos.Compiler.DebugStub\", "*.xs");
      foreach (var xFile in xFiles) {
        Echo("Compiling " + Path.GetFileName(xFile));
        string xDest = Path.ChangeExtension(xFile, ".cs");
        if (File.Exists(xDest)) {
          ResetReadOnly(xDest);
        }
        // We dont ref the X# asm directly because then we could not compile it without dynamic loading.
        // This way we can build it and call it directly.
        StartConsole(mOutputDir + @"\xsc.exe", Quoted(xFile) + @" Cosmos.Debug.DebugStub");
      }
    }

    void CompileCosmos() {
      Section("Compiling Cosmos");

      MsBuild(Path.Combine(mCosmosDir, @"source\Build.sln"), "Debug");
    }

    void CopyTemplates() {
      Section("Copying Templates");

      CD(mOutputDir);
      SrcPath = Path.Combine(mCosmosDir, @"source2\VSIP\Cosmos.VS.Package\obj\x86\Debug");
      Copy("CosmosProject (C#).zip", true);
      Copy("CosmosKernel (C#).zip", true);
      Copy("CosmosProject (F#).zip", true);
      Copy("Cosmos.zip", true);
      Copy("CosmosProject (VB).zip", true);
      Copy("CosmosKernel (VB).zip", true);
      Copy(mCosmosDir + @"source\XSharp.VS\Template\XSharpFileItem.zip", true);
    }

    void CreateSetup() {
      Section("Creating Setup");

      string xISCC = Path.Combine(mInnoPath, "ISCC.exe");
      if (!File.Exists(xISCC)) {
        throw new Exception("Cannot find Inno setup.");
      }
      string xCfg = App.IsUserKit ? "UserKit" : "DevKit";
      StartConsole(xISCC, @"/Q " + Quoted(mInnoFile) + " /dBuildConfiguration=" + xCfg);

      if (App.IsUserKit) {
        File.Delete(mInnoFile);
      }
    }

    void LaunchVS() {
      Section("Launching Visual Studio");

      string xVisualStudio = Paths.VSInstall + @"\devenv.exe";
      if (!File.Exists(xVisualStudio)) {
        throw new Exception("Cannot find Visual Studio.");
      }

      if (App.ResetHive) {
        Echo("Resetting hive");
        Start(xVisualStudio, @"/setup /rootsuffix Exp /ranu");
      }

      Echo("Launching Visual Studio");
      // Fix issue #15565
      Start(xVisualStudio, Quoted(mCosmosDir + @"\source\Cosmos.sln"), false, true);
    }

    void RunSetup() {
      Section("Running Setup");

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
        if (WaitForStart("CosmosUserKit-" + mReleaseNo, xSeconds * 1000)) {
          throw new Exception("Setup did not start.");
        }
        Echo("Setup is running. " + DateTime.Now.Subtract(xTimed).ToString(@"ss\.fff"));

        // Scheduler starts it an exits, but we need to wait for the setup itself to exit before proceding
        Echo("Waiting for Setup to complete.");
        WaitForExit("CosmosUserKit-" + mReleaseNo);
      } else {
        Start(mCosmosDir + @"Setup\Output\CosmosUserKit-" + mReleaseNo + ".exe", @"/SILENT");
      }
    }

    void Done() {
      Section("Build Complete!");
    }
  }
}