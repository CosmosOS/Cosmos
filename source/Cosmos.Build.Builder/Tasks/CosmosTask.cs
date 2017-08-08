using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace Cosmos.Build.Builder.Tasks
{
  /// <summary>
  /// Cosmos task.
  /// </summary>
  /// <seealso cref="Task" />
  public class CosmosTask : Task
  {
    private NugetTask mNugetTask;
    private string mInnoPath;
    private string mInnoFile;

    private BuildState mBuildState;
    private int mReleaseNo;

    public CosmosTask(string aCosmosDir, int aReleaseNo)
      : base(aCosmosDir)
    {
      mReleaseNo = aReleaseNo;
      mInnoFile = Path.Combine(mCosmosPath, @"Setup\Cosmos.iss");
    }

    /// <summary>
    /// Get name of the setup file based on release number and the current setting.
    /// </summary>
    /// <param name="releaseNumber">Release number for the current setup.</param>
    /// <returns>Name of the setup file.</returns>
    public static string GetSetupName(int releaseNumber)
    {
      string setupName = $"CosmosUserKit-{releaseNumber}-vs2017";

      if (App.UseVsHive)
      {
        setupName += "Exp";
      }

      return setupName;
    }

    private void CleanDirectory(string aName, string aPath)
    {
      if (Directory.Exists(aPath))
      {
        Log.WriteLine("Cleaning up existing " + aName + " directory.");
        Directory.Delete(aPath, true);
      }

      Log.WriteLine("Creating " + aName + " as " + aPath);
      Directory.CreateDirectory(aPath);
    }

    protected override void DoRun()
    {
      if (PrereqsOK())
      {
        Section("Init Directories");
        CleanDirectory("VSIP", mVsipPath);
        if (!App.IsUserKit)
        {
          CleanDirectory("User Kit", mAppDataPath);
        }

        NugetTask.RunTasks(mCosmosPath);
        MSBuildTask.RunTasks(mCosmosPath);

        CreateSetup();

        if (!App.IsUserKit)
        {
          RunSetup();
          WriteDevKit();
          if (!App.DoNotLaunchVS)
          {
            LaunchVS();
          }
        }
        Done();
      }
    }

    protected int NumProcessesContainingName(string name)
    {
      return (from x in Process.GetProcesses() where x.ProcessName.Contains(name) select x).Count();
    }

    protected void CheckIfBuilderRunning()
    {
      //Check for builder process
      Log.WriteLine("Check if Builder is running.");
      // Check > 1 so we exclude ourself.
      if (NumProcessesContainingName("Cosmos.Build.Builder") > 1)
      {
        throw new Exception("Another instance of builder is running.");
      }
    }

    protected void CheckIfUserKitRunning()
    {
      Log.WriteLine("Check if User Kit Installer is already running.");
      if (NumProcessesContainingName("CosmosUserKit") > 0)
      {
        throw new Exception("Another instance of the user kit installer is running.");
      }
    }

    protected void CheckIfVSRunning()
    {
      int xSeconds = 500;

      if (Debugger.IsAttached)
      {
        Log.WriteLine("Check if Visual Studio is running is ignored by debugging of Builder.");
      }
      else
      {
        Log.WriteLine("Check if Visual Studio is running.");
        if (IsRunning("devenv"))
        {
          Log.WriteLine("--Visual Studio is running.");
          Log.WriteLine("--Waiting " + xSeconds + " seconds to see if Visual Studio exits.");
          // VS doesnt exit right away and user can try devkit again after VS window has closed but is still running.
          // So we wait a few seconds first.
          if (WaitForExit("devenv", xSeconds * 1000))
          {
            throw new Exception("Visual Studio is running. Please close it or kill it in task manager.");
          }
        }
      }
    }

    protected void NotFound(string aName)
    {
      AddExceptionMessage("Prerequisite '" + aName + "' not found.");
      mBuildState = BuildState.PrerequisiteMissing;
    }

    protected bool PrereqsOK()
    {
      Section("Check Prerequisites");

      CheckIfUserKitRunning();
      CheckIfVSRunning();
      CheckIfBuilderRunning();

      CheckForNetCore();
      CheckForVisualStudioExtensionTools();
      CheckForInno();

      return mBuildState != BuildState.PrerequisiteMissing;
    }

    private void CheckForInno()
    {
      Log.WriteLine("Check for Inno Setup");
      using (var xKey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\Inno Setup 5_is1", false))
      {
        if (xKey == null)
        {
          AddExceptionMessage("Cannot find Inno Setup.");
          mBuildState = BuildState.PrerequisiteMissing;
          return;
        }
        mInnoPath = (string)xKey.GetValue("InstallLocation");
        if (string.IsNullOrWhiteSpace(mInnoPath))
        {
          AddExceptionMessage("Cannot find Inno Setup.");
          mBuildState = BuildState.PrerequisiteMissing;
          return;
        }
      }

      Log.WriteLine("Check for Inno Preprocessor");
      if (!File.Exists(Path.Combine(mInnoPath, "ISPP.dll")))
      {
        AddExceptionMessage("Inno Preprocessor not detected.");
        mBuildState = BuildState.PrerequisiteMissing;
        return;
      }
    }

    private void CheckForNetCore()
    {
      Log.WriteLine("Check for .NET Core");

      if (!VSHelper.IsWorkloadInstalled("Microsoft.VisualStudio.Workload.NetCoreTools"))
      {
        AddExceptionMessage(".NET Core not detected.");
        mBuildState = BuildState.PrerequisiteMissing;
      }
    }

    private void CheckForVisualStudioExtensionTools()
    {
      Log.WriteLine("Check for Visual Studio Extension Tools");

      if (!VSHelper.IsWorkloadInstalled("Microsoft.VisualStudio.Workload.VisualStudioExtension"))
      {
        AddExceptionMessage("Visual Studio Extension tools not detected.");
        mBuildState = BuildState.PrerequisiteMissing;
      }
    }

    private void WriteDevKit()
    {
      Section("Write Dev Kit to Registry");

      // Inno deletes this from registry, so we must add this after.
      // We let Inno delete it, so if user runs it by itself they get
      // only UserKit, and no DevKit settings.
      // HKCU instead of HKLM because builder does not run as admin.
      //
      // HKCU is not redirected.
      using (var xKey = Registry.CurrentUser.CreateSubKey(@"Software\Cosmos"))
      {
        xKey.SetValue("DevKit", mCosmosPath);
      }
    }

    private void CreateSetup()
    {
      Section("Creating Setup");

      string xISCC = Path.Combine(mInnoPath, "ISCC.exe");
      if (!File.Exists(xISCC))
      {
        AddExceptionMessage("Cannot find Inno setup.");
        return;
      }

      string xCfg = App.IsUserKit ? "UserKit" : "DevKit";
      string vsVersionConfiguration = "vs2017";

      // Use configuration which will install to the VS Exp Hive
      if (App.UseVsHive)
      {
        vsVersionConfiguration += "Exp";
      }
      Log.WriteLine($"  {xISCC} /Q {Quoted(mInnoFile)} /dBuildConfiguration={xCfg} /dVSVersion={vsVersionConfiguration} /dChangeSetVersion={Quoted(mReleaseNo.ToString())}");
      StartConsole(xISCC, $"/Q {Quoted(mInnoFile)} /dBuildConfiguration={xCfg} /dVSVersion={vsVersionConfiguration} /dChangeSetVersion={Quoted(mReleaseNo.ToString())}");
    }

    private void LaunchVS()
    {
      Section("Launching Visual Studio");

      string xVisualStudio = Path.Combine(VSHelper.VSPath, "Common7", "IDE", "devenv.exe");
      if (!File.Exists(xVisualStudio))
      {
        AddExceptionMessage("Cannot find Visual Studio.");
        return;
      }

      if (App.ResetHive)
      {
        Log.WriteLine("Resetting hive");
        Start(xVisualStudio, @"/setup /rootsuffix Exp /ranu");
      }

      Log.WriteLine("Launching Visual Studio");
      Start(xVisualStudio, Quoted(mCosmosPath + @"Kernel.sln"), false, true);
    }

    private void RunSetup()
    {
      Section("Running Setup");

      // These cache in RAM which cause problems, so we kill them if present.
      KillProcesses("dotnet");
      KillProcesses("msbuild");

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
        Log.WriteLine("Waiting " + xSeconds + " seconds for Setup to start.");
        if (WaitForStart(setupName, xSeconds * 1000))
        {
          AddExceptionMessage("Setup did not start.");
          return;
        }
        Log.WriteLine("Setup is running. " + DateTime.Now.Subtract(xTimed).ToString(@"ss\.fff"));

        // Scheduler starts it and exits, but we need to wait for the setup itself to exit before proceding
        Log.WriteLine("Waiting for Setup to complete.");
        WaitForExit(setupName);
      }
      else
      {
        Start(mCosmosPath + @"Setup\Output\" + setupName + ".exe", @"/SILENT");
      }
    }

    private void Done()
    {
      Section("Build Complete!");
    }
  }
}
