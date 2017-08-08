using System;
using System.IO;

namespace Cosmos.Build.Builder.Tasks
{
  public class MSBuildTask : Task
  {
    private readonly string mMSBuildPath;

    private MSBuildTask(string aCosmosDir)
      : base(aCosmosDir)
    {
      string xMSBuild = Path.Combine(VSHelper.VSPath, "MSBuild", "15.0", "Bin", "msbuild.exe");
      if (File.Exists(xMSBuild))
      {
        mMSBuildPath = xMSBuild;
      }
      else
      {
        AddExceptionMessage("Could not find MSBuld");
        throw new FileNotFoundException("Could not find MSBuild", xMSBuild);
      }
    }

    public static void RunTasks(string aCosmosPath)
    {
      var xTask = new MSBuildTask(aCosmosPath);
      xTask.Run();
    }

    protected override void DoRun()
    {
      string xVsipDir = Path.Combine(mCosmosPath, "Build", "VSIP");
      string xNugetPkgDir = Path.Combine(xVsipDir, "KernelPackages");
      string xVersion = DateTime.Now.ToString("yyyy.MM.dd");


      Section("Build Cosmos");
      // Build.sln is the old master but because of how VS manages refs, we have to hack
      // this short term with the new slns.
      Build(Path.Combine(mCosmosPath, @"Build.sln"), "Debug");

      Section("Publish Tools");
      Publish(Path.Combine(mSourcePath, "Cosmos.Build.MSBuild"), Path.Combine(xVsipDir, "MSBuild"));
      Publish(Path.Combine(mSourcePath, "IL2CPU"), Path.Combine(xVsipDir, "IL2CPU"));
      Publish(Path.Combine(mSourcePath, "XSharp.Compiler"), Path.Combine(xVsipDir, "XSharp"));
      Publish(Path.Combine(mCosmosPath, "Tools", "NASM"), Path.Combine(xVsipDir, "NASM"));

      Section("Pack Kernel");
      //
      Pack(Path.Combine(mSourcePath, "Cosmos.Common"), xNugetPkgDir, xVersion);
      //
      Pack(Path.Combine(mSourcePath, "Cosmos.Core"), xNugetPkgDir, xVersion);
      Pack(Path.Combine(mSourcePath, "Cosmos.Core_Plugs"), xNugetPkgDir, xVersion);
      Pack(Path.Combine(mSourcePath, "Cosmos.Core_Asm"), xNugetPkgDir, xVersion);
      //
      Pack(Path.Combine(mSourcePath, "Cosmos.HAL2"), xNugetPkgDir, xVersion);
      //
      Pack(Path.Combine(mSourcePath, "Cosmos.System2"), xNugetPkgDir, xVersion);
      Pack(Path.Combine(mSourcePath, "Cosmos.System2_Plugs"), xNugetPkgDir, xVersion);
      //
      Pack(Path.Combine(mSourcePath, "Cosmos.Debug.Kernel"), xNugetPkgDir, xVersion);
      Pack(Path.Combine(mSourcePath, "Cosmos.Debug.Kernel.Plugs.Asm"), xNugetPkgDir, xVersion);
      //
      Pack(Path.Combine(mSourcePath, "Cosmos.IL2CPU.API"), xNugetPkgDir, xVersion);

    }

    private void Build(string aSlnFile, string aBuildCfg)
    {
      string xParams = $"{Quoted(aSlnFile)} /nodeReuse:False /nologo /maxcpucount /p:Configuration={Quoted(aBuildCfg)} /p:Platform={Quoted("Any CPU")} /p:OutputPath={Quoted(mVsipPath)}";
      if (!App.NoMSBuildClean)
      {
        StartConsole(mMSBuildPath, $"/t:Clean {xParams}");
      }
      StartConsole(mMSBuildPath, $"/t:Build {xParams}");
    }

    private void Pack(string aProject, string aDestDir, string aVersion)
    {
      string xParams = $"{Quoted(aProject)} /nodeReuse:False /nologo /maxcpucount /t:Restore;Pack /p:PackageVersion={Quoted(aVersion)} /p:PackageOutputPath={Quoted(aDestDir)}";
      StartConsole(mMSBuildPath, xParams);
    }

    private void Publish(string aProject, string aDestDir)
    {
      string xParams = $"{Quoted(aProject)} /nodeReuse:False /nologo /maxcpucount /t:Publish /p:RuntimeIdentifier=win7-x86 /p:PublishDir={Quoted(aDestDir)}";
      StartConsole(mMSBuildPath, xParams);
    }
  }
}
