using Cosmos.Build.Installer;
using NuGet.Configuration;
using System;
using System.Diagnostics;
using System.IO;

namespace Cosmos.Build.Builder.Tasks
{
  public class NugetTask : Task
  {
    private readonly string mNugetPath;

    private NugetTask(string aCosmosDir)
      : base(aCosmosDir)
    {
      string xNuget = Path.Combine(mCosmosPath, "Build", "Tools", "nuget.exe");
      if (File.Exists(xNuget))
      {
        mNugetPath = xNuget;
      }
      else
      {
        AddExceptionMessage("Could not find NuGet");
        throw new FileNotFoundException("Could not find NuGet", xNuget);
      }
    }

    public static void RunTasks(string aCosmosPath)
    {
      var xTask = new NugetTask(aCosmosPath);
      xTask.Run();
    }

    protected override void DoRun()
    {
      Section("Update NuGet Executable");
      UpdateNuGet();

      Section("Clean NuGet Local Feed");
      CleanLocalPackageFeed();

      Section("Restore NuGet Packages");
      Restore(Path.Combine(mCosmosPath, @"Build.sln"));
    }

    private void Restore(string project)
    {
      string xRestoreParams = $"restore {Quoted(project)}";
      StartConsole(mNugetPath, xRestoreParams);
    }

    private void UpdateNuGet()
    {
      const string xUpdateParams = "update -self";
      StartConsole(mNugetPath, xUpdateParams);
    }

    private void CleanLocalPackageFeed()
    {
      string xListParams = $"sources List";
      var xStart = new ProcessStartInfo();
      xStart.FileName = mNugetPath;
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
          while (true)
          {
            string xLine = xReader.ReadLine();
            if (xLine == null)
            {
              break;
            }
            if (xLine.Contains("Cosmos Local Package Feed"))
            {
              string xUninstallParams = $"sources Remove -Name \"Cosmos Local Package Feed\"";
              StartConsole(mNugetPath, xUninstallParams);
            }
          }
        }
      }

      // Clean Cosmos packages from NuGet cache
      string xGlobalFolder = SettingsUtility.GetGlobalPackagesFolder(Settings.LoadDefaultSettings(Environment.SystemDirectory));

      // Later we should specify the packages, currently we're moving to gen3 so package names are a bit unstable
      foreach (var xFolder in Directory.EnumerateDirectories(xGlobalFolder))
      {
        if (xFolder.StartsWith("Cosmos", StringComparison.InvariantCultureIgnoreCase))
        {
          string xFullPath = Path.Combine(xGlobalFolder, xFolder);
          Directory.Delete(xFullPath, true);
        }
      }
    }
  }
}