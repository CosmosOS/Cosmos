using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Cosmos.Build.Installer;
using System.IO;
using Microsoft.Win32;

namespace Cosmos.Build.Builder
{
    /// <summary>
    /// Cosmos task.
    /// </summary>
    /// <seealso cref="Cosmos.Build.Installer.Task" />
    public class CosmosTask : Task
    {
        protected string mCosmosDir;
        protected string mOutputDir;
        protected BuildState mBuildState;
        protected string mAppDataDir;
        protected int mReleaseNo;
        protected string mInnoFile;
        protected string mInnoPath;
        // Instead of throwing every exception, we collect them in a list
        protected List<string> mExceptionList = new List<string>();
        public string InnoScriptTargetFile = "Current.iss";

        public CosmosTask(string aCosmosDir, int aReleaseNo)
        {
            mCosmosDir = aCosmosDir;
            mAppDataDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "Cosmos User Kit");
            mReleaseNo = aReleaseNo;
            mInnoFile = Path.Combine(mCosmosDir, @"Setup\Cosmos.iss");
        }

        /// <summary>
        /// Get name of the setup file based on release number and the current setting.
        /// </summary>
        /// <param name="releaseNumber">Release number for the current setup.</param>
        /// <returns>Name of the setup file.</returns>
        public static string GetSetupName(int releaseNumber)
        {
            var setupName = "CosmosUserKit-" + releaseNumber;
            switch (App.VsVersion)
            {
                case VsVersion.Vs2015:
                    setupName += "-vs2015";
                    break;

            }

            if (App.UseVsHive)
            {
                setupName += "Exp";
            }

            return setupName;
        }

        void CleanupVSIPFolder()
        {
            if (Directory.Exists(mOutputDir))
            {
                Section("Cleaning up VSIP Folder");

                // Make sure no files are left, else things can be not be rebuilt and when adding
                // new items this can cause issues.
                Echo("Deleting build output directory.");
                Echo("  " + mOutputDir);
                Directory.Delete(mOutputDir, true);
            }
        }

        void CleanupAlreadyInstalled()
        {
            //in case install folder is the same like the last installation, inno setup delete already this path!
            // mean this is normally not needed, what do you think?
            if (Directory.Exists(mAppDataDir))
            {
                Section("Cleaning up currently installed user kit directory");
                Echo("  " + mAppDataDir);
                Directory.Delete(mAppDataDir, true);
            }
        }

        protected override List<string> DoRun()
        {
            mOutputDir = Path.Combine(mCosmosDir, @"Build\VSIP");
            if (!App.TestMode)
            {
                CheckPrereqs();
                // No point in continuing if Prerequisites are missing
                // Could potentially add more State checks in the future, but for now
                // only the prerequisites are handled...
                if (mBuildState != BuildState.PrerequisiteMissing)
                {
                    CleanupVSIPFolder();

                    CompileCosmos();
                    CopyTemplates();

                    CreateScriptToUseChangesetWhichTaskIsUse();

                    CreateSetup();
                    if (!App.IsUserKit)
                    {
                        CleanupAlreadyInstalled();
                        RunSetup();
                        WriteDevKit();
                        if (!App.DoNotLaunchVS)
                        {
                            LaunchVS();
                        }
                    }
                }
                Done();
            }
            else
            {
                Section("Testing...");
                //Uncomment bits that you want to test...
                //CheckForInno();
                CheckPrereqs();
                if (mBuildState != BuildState.PrerequisiteMissing)
                {
                    Echo("all checks succeeded");
                }
                //Cleanup();                                   

                //CompileCosmos();                             
                //CopyTemplates();                             
                //if (App.IsUserKit)
                //{
                //    CreateUserKitScript();                   
                //}
                //CreateSetup();                               
                //if (!App.IsUserKit)
                //{
                //    RunSetup();                              
                //    WriteDevKit();                           
                //    if (!App.DoNotLaunchVS) { LaunchVS(); }  
                //}

                //Done();
            }
            return mExceptionList;
        }

        protected void MsBuild(string aSlnFile, string aBuildCfg)
        {
            string xMsBuild = Path.Combine(Paths.ProgFiles32, @"MSBuild\14.0\Bin\msbuild.exe");
            string xParams = $"{Quoted(aSlnFile)} " +
                             "/nologo " +
                             "/maxcpucount " +
                             $"/p:Configuration={Quoted(aBuildCfg)} " +
                             $"/p:Platform={Quoted("Any CPU")} " +
                             $"/p:OutputPath={Quoted(mOutputDir)}";

            if (!App.NoMsBuildClean)
            {
                StartConsole(xMsBuild, $"/t:Clean {xParams}");
            }
            StartConsole(xMsBuild, $"/t:Build {xParams}");
        }

        protected int NumProcessesContainingName(string name)
        {
            return (from x in Process.GetProcesses() where x.ProcessName.Contains(name) select x).Count();
        }

        protected bool IsVMWareInstalled()
        {
            //Check registry keys
            if (CheckForInstall("VMware Workstation", false)) {
				return true;
			}
            if (CheckForInstall("VMware Player", false)) {
				return true;
			}
            if (CheckForInstall("VMwarePlayer_x64", false)) {
				return true;
			}

            try //Try/catch block since the reg key might not exist, we might not have perms etc.
            {
                using (var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\VMware, Inc.\VMware Player"))
                {
                    return true; //Just assume that because we have the key, we've installed
                    //return (key.GetValue("ProductCode") != null); //On successful install, ProductCode should be set (we don't care what the value is, but we care that it exists)
                }
            }
            catch
            { //TODO: check directories?
                return false;
            }
        }

        protected bool CheckForInstall(string aCheck, bool aCanThrow)
        {
            return CheckForProduct(aCheck, aCanThrow, @"SOFTWARE\Classes\Installer\Products\", "ProductName");
        }

        protected bool CheckForUninstall(string aCheck, bool aCanThrow)
        {
            return CheckForProduct(aCheck, aCanThrow, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\",
                "DisplayName");
        }

        protected bool CheckForProduct(string aCheck, bool aCanThrow, string aKey, string aValueName)
        {
            Echo("Checking for " + aCheck);
            string xCheck = aCheck.ToUpper();
            string[] xKeys;
            using (var xKey = Registry.LocalMachine.OpenSubKey(aKey, false))
            {
                xKeys = xKey.GetSubKeyNames();
            }
            foreach (string xSubKey in xKeys)
            {
                using (var xKey = Registry.LocalMachine.OpenSubKey(aKey + xSubKey))
                {
                    string xValue = (string) xKey.GetValue(aValueName);
                    if (xValue != null && xValue.ToUpper().Contains(xCheck))
                    {
                        if (mBuildState != BuildState.PrerequisiteMissing)
                        {
                            mBuildState = BuildState.Running;
                            return true;
                        }
                        else
                            return false;
                    }
                }
            }

            if (aCanThrow)
            {
                NotFound(aCheck);
            }
            return false;
        }

        protected void CheckNet35Sp1()
        {
            Echo("Checking for .NET 3.5 SP1");
            bool xInstalled = false;
            using (var xKey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\NET Framework Setup\NDP\v3.5"))
            {
                if (xKey != null)
                {
                    xInstalled = (int) xKey.GetValue("SP", 0) >= 1;
                }
            }
            if (!xInstalled)
            {
                NotFound(".NET 3.5 SP1");
                mBuildState = BuildState.PrerequisiteMissing;
            }
        }

        protected void CheckNet403()
        {
            Echo("Checking for .NET 4.03");
            if (
                Registry.LocalMachine.OpenSubKey(
                    @"SOFTWARE\Microsoft\.NETFramework\v4.0.30319\SKUs\.NETFramework,Version=v4.0.3") == null)
            {
                NotFound(".NET 4.03 Full Install (not client)");
            }
        }

        protected void CheckOS()
        {
            Echo("Checking Operating System");
            var xOsInfo = System.Environment.OSVersion;
            if (xOsInfo.Platform != PlatformID.Win32NT)
            {
                NotFound("Supported OS");
            }
            decimal xVer = decimal.Parse(xOsInfo.Version.Major + "." + xOsInfo.Version.Minor,
                System.Globalization.CultureInfo.InvariantCulture);
            // 6.0 Vista
            // 6.1 2008
            // 6.2 Windows 7
            // 6.3 Windows 8
            // 6.4 Windows 10
            if (xVer < 6.0m)
            {
                NotFound("Minimum Supported OS is Vista/2008");
            }
        }

        protected void CheckIfBuilderRunning()
        {
            //Check for builder process
            Echo("Checking if Builder is already running.");
            // Check > 1 so we exclude ourself.
            if (NumProcessesContainingName("Cosmos.Build.Builder") > 1)
            {
                throw new Exception("Another instance of builder is running.");
            }
        }

        protected void CheckIfUserKitRunning()
        {
            Echo("Check if User Kit Installer is already running.");
            if (NumProcessesContainingName("CosmosUserKit") > 1)
            {
                throw new Exception("Another instance of the user kit installer is running.");
            }
        }

        protected void CheckIsVsRunning()
        {
            int xSeconds = 500;
            if (App.IgnoreVS)
            {
                return;
            }

            if (AreWeNowDebugTheBuilder())
            {
                Echo("Checking if Visual Studio is running is ignored by debugging of Builder.");
            }
            else
            {
                Echo("Checking if Visual Studio is running.");
                if (IsRunning("devenv"))
                {
                    Echo("--Visual Studio is running.");
                    Echo("--Waiting " + xSeconds + " seconds to see if Visual Studio exits.");
                    // VS doesnt exit right away and user can try devkit again after VS window has closed but is still running.
                    // So we wait a few seconds first.
                    if (WaitForExit("devenv", xSeconds*1000))
                    {
                        throw new Exception("Visual Studio is running. Please close it or kill it in task manager.");
                    }
                }
            }
        }

        private bool AreWeNowDebugTheBuilder()
        {
            return Process.GetCurrentProcess().ProcessName.EndsWith(".vshost");
        }

        protected void NotFound(string aName)
        {
            mExceptionList.Add("Prerequisite '" + aName + "' not found.");
            mBuildState = BuildState.PrerequisiteMissing;
        }

        protected void CheckPrereqs()
        {
            Section("Checking Prerequisites");
            Echo("Note: This check only prerequisites for building, please see website for full list.");

            Echo("Checking for x86 run.");
            if (!AmRunning32Bit())
            {
                mExceptionList.Add("Builder must run as x86");
                mBuildState = BuildState.PrerequisiteMissing;
            }

            // We assume they have normal .NET stuff if user was able to build the builder...

            CheckOS();
            CheckIfUserKitRunning();
            CheckIsVsRunning();
            CheckIfBuilderRunning();

            switch (App.VsVersion)
            {
                case VsVersion.Vs2015:
                    CheckVs2015();
                    CheckForInstall("Microsoft Visual Studio 2015 SDK - ENU", true);
                    break;
                default:
                    throw new NotImplementedException();
            }

            //works also without, only close of VMWare is not working!
            CheckNet35Sp1(); // Required by VMWareLib and other stuff
            CheckNet403();
            CheckForInno();
            bool vmWareInstalled = IsVMWareInstalled();
            bool bochsInstalled = IsBochsInstalled();
            
            if (!vmWareInstalled && !bochsInstalled)
            {
                NotFound("VMWare or Bochs");
            }
            // VIX is installed with newer VMware Workstations (8+ for sure). VMware player does not install it?
            // We need to just watch this and adjust as needed.
            //CheckForInstall("VMWare VIX", true);
        }

        /// <summary>Check for Bochs being installed.</summary>
        private static bool IsBochsInstalled()
        {
            try
            {
                using (
                    var runCommandRegistryKey = Registry.ClassesRoot.OpenSubKey(@"BochsConfigFile\shell\Run\command",
                        false))
                {
                    if (null == runCommandRegistryKey)
                    {
                        return false;
                    }
                    string commandLine = (string) runCommandRegistryKey.GetValue(null, null);
                    if (null != commandLine)
                    {
                        commandLine = commandLine.Trim();
                    }
                    if (string.IsNullOrEmpty(commandLine))
                    {
                        return false;
                    }
                    // Now perform some parsing on command line to discover full exe path.
                    string candidateFilePath;
                    int commandLineLength = commandLine.Length;
                    if ('"' == commandLine[0])
                    {
                        // Seek for a non escaped double quote.
                        int lastDoubleQuoteIndex = 1;
                        for (; lastDoubleQuoteIndex < commandLineLength; lastDoubleQuoteIndex++)
                        {
                            if ('"' != commandLine[lastDoubleQuoteIndex])
                            {
                                continue;
                            }
                            if ('\\' != commandLine[lastDoubleQuoteIndex - 1])
                            {
                                break;
                            }
                        }
                        if (lastDoubleQuoteIndex >= commandLineLength)
                        {
                            return false;
                        }
                        candidateFilePath = commandLine.Substring(1, lastDoubleQuoteIndex - 1);
                    }
                    else
                    {
                        // Seek for first separator character.
                        int firstSeparatorIndex = 0;
                        for (; firstSeparatorIndex < commandLineLength; firstSeparatorIndex++)
                        {
                            if (char.IsSeparator(commandLine[firstSeparatorIndex]))
                            {
                                break;
                            }
                        }
                        if (firstSeparatorIndex >= commandLineLength)
                        {
                            return false;
                        }
                        candidateFilePath = commandLine.Substring(0, firstSeparatorIndex);
                    }
                    return File.Exists(candidateFilePath);
                }
            }
            catch
            {
                return false;
            }
        }

        void CheckForInno()
        {
            Echo("Checking for Inno Setup");
            using (
                var xKey =
                    Registry.LocalMachine.OpenSubKey(
                        @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\Inno Setup 5_is1", false))
            {
                if (xKey == null)
                {
                    mExceptionList.Add("Cannot find Inno Setup.");
                    mBuildState = BuildState.PrerequisiteMissing;
                    return;
                }
                mInnoPath = (string) xKey.GetValue("InstallLocation");
                if (string.IsNullOrWhiteSpace(mInnoPath))
                {
                    mExceptionList.Add("Cannot find Inno Setup.");
                    mBuildState = BuildState.PrerequisiteMissing;
                    return;
                }
            }

            Echo("Checking for Inno Preprocessor");
            if (!File.Exists(Path.Combine(mInnoPath, "ISPP.dll")))
            {
                mExceptionList.Add("Inno Preprocessor not detected.");
                mBuildState = BuildState.PrerequisiteMissing;
                return;
            }
        }

        void CheckVs2015()
        {
            Echo("Checking for Visual Studio 2015");
            string key = @"SOFTWARE\Microsoft\VisualStudio\14.0";
            if (Environment.Is64BitOperatingSystem)
                key = @"SOFTWARE\Wow6432Node\Microsoft\VisualStudio\14.0";
            using (var xKey = Registry.LocalMachine.OpenSubKey(key))
            {
                string xDir = (string) xKey.GetValue("InstallDir");
                if (String.IsNullOrWhiteSpace(xDir))
                {
                    mExceptionList.Add("Visual Studio 2015 not detected!");
                    mBuildState = BuildState.PrerequisiteMissing;
                }
            }
        }

        void WriteDevKit()
        {
            Section("Writing Dev Kit to Registry");

            // Inno deletes this from registry, so we must add this after.
            // We let Inno delete it, so if user runs it by itself they get
            // only UserKit, and no DevKit settings.
            // HKCU instead of HKLM because builder does not run as admin.
            //
            // HKCU is not redirected.
            using (var xKey = Registry.CurrentUser.CreateSubKey(@"Software\Cosmos"))
            {
                xKey.SetValue("DevKit", mCosmosDir);
            }
        }

        void CreateScriptToUseChangesetWhichTaskIsUse()
        {
            Section("Creating Inno Setup Script");

            // Read in iss file
            using (var xSrc = new StreamReader(mInnoFile))
            {
                mInnoFile = Path.Combine(Path.GetDirectoryName(mInnoFile), InnoScriptTargetFile);
                // Write out new iss
                using (var xDest = new StreamWriter(mInnoFile))
                {
                    string xLine;
                    while ((xLine = xSrc.ReadLine()) != null)
                    {
                        if (xLine.StartsWith("#define ChangeSetVersion ", StringComparison.InvariantCultureIgnoreCase))
                        {
                            xDest.WriteLine("#define ChangeSetVersion " + Quoted(mReleaseNo.ToString()));
                        }
                        else
                        {
                            xDest.WriteLine(xLine);
                        }
                    }
                }
            }
        }

        void CompileCosmos()
        {
            Section("Compiling Cosmos");

            MsBuild(Path.Combine(mCosmosDir, @"source\Build.sln"), "Debug");
        }

        void CopyTemplates()
        {
            Section("Copying Templates");

            CD(mOutputDir);
            SrcPath = Path.Combine(mCosmosDir, @"source\Cosmos.VS.Package\obj\Debug");
            Copy("CosmosProject (C#).zip", true);
            Copy("CosmosKernel (C#).zip", true);
            Copy("CosmosProject (F#).zip", true);
            Copy("Cosmos.zip", true);
            Copy("CosmosProject (VB).zip", true);
            Copy("CosmosKernel (VB).zip", true);
            Copy(mCosmosDir + @"source\XSharp.VS\Template\XSharpFileItem.zip", true);
        }

        void CreateSetup()
        {
            Section("Creating Setup");


            string xISCC = Path.Combine(mInnoPath, "ISCC.exe");
            if (!File.Exists(xISCC))
            {
                mExceptionList.Add("Cannot find Inno setup.");
                return;
            }
            string xCfg = App.IsUserKit ? "UserKit" : "DevKit";
            string vsVersionConfiguration = "vs2015";
            switch (App.VsVersion)
            {
                case VsVersion.Vs2015:
                    vsVersionConfiguration = "vs2015";
                    break;
            }
            // Use configuration which will instal to the VS Exp Hive
            if (App.UseVsHive)
            {
                vsVersionConfiguration += "Exp";
            }
            StartConsole(xISCC,
                @"/Q " + Quoted(mInnoFile) + " /dBuildConfiguration=" + xCfg + " /dVsVersion=" + vsVersionConfiguration);

            if (App.IsUserKit)
            {
                File.Delete(mInnoFile);
            }
        }

        void LaunchVS()
        {
            Section("Launching Visual Studio");

            string xVisualStudio = Paths.VSInstall + @"\devenv.exe";
            if (!File.Exists(xVisualStudio))
            {
                mExceptionList.Add("Cannot find Visual Studio.");
                return;
            }

            if (App.ResetHive)
            {
                Echo("Resetting hive");
                Start(xVisualStudio, @"/setup /rootsuffix Exp /ranu");
            }

            Echo("Launching Visual Studio");
            // Fix issue #15565
            Start(xVisualStudio, Quoted(mCosmosDir + @"\source\Cosmos.sln"), false, true);
        }

        void RunSetup()
        {
            Section("Running Setup");

            string setupName = GetSetupName(mReleaseNo);

            if (App.UseTask)
            {
                // This is a hack to avoid the UAC dialog on every run which can be very disturbing if you run
                // the dev kit a lot.
                Start(@"schtasks.exe", @"/run /tn " + Quoted("CosmosSetup"), true, false);

                // Must check for start before stop, else on slow machines we exit quickly because Exit is found before
                // it starts.
                // Some slow user PCs take around 5 seconds to start up the task...
                int xSeconds = 10;
                var xTimed = DateTime.Now;
                Echo("Waiting " + xSeconds + " seconds for Setup to start.");
                if (WaitForStart(setupName, xSeconds*1000))
                {
                    mExceptionList.Add("Setup did not start.");
                    return;
                }
                Echo("Setup is running. " + DateTime.Now.Subtract(xTimed).ToString(@"ss\.fff"));

                // Scheduler starts it an exits, but we need to wait for the setup itself to exit before proceding
                Echo("Waiting for Setup to complete.");
                WaitForExit(setupName);
            }
            else
            {
                Start(mCosmosDir + @"Setup\Output\" + setupName + ".exe", @"/SILENT");
            }
        }

        void Done()
        {
            Section("Build Complete!");
        }
    }
}