using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.Setup.Configuration;

using Cosmos.Build.Builder.BuildTasks;
using Cosmos.Build.Builder.Dependencies;
using Cosmos.Build.Builder.Services;

namespace Cosmos.Build.Builder
{
    internal class CosmosBuildDefinition : IBuildDefinition
    {
        private IInnoSetupService _innoSetupService;
        private IMSBuildService _msBuildService;
        private ISetupInstance2 _visualStudioInstance;

        public CosmosBuildDefinition(
            IInnoSetupService innoSetupService,
            IMSBuildService msBuildService,
            ISetupInstance2 visualStudioInstance)
        {
            _innoSetupService = innoSetupService;
            _msBuildService = msBuildService;
            _visualStudioInstance = visualStudioInstance;
        }

        public IEnumerable<IDependency> GetDependencies()
        {
            yield return new VisualStudioDependency(_visualStudioInstance);
            yield return new VisualStudioWorkloadsDependency(_visualStudioInstance);
            yield return new InnoSetupDependency(_innoSetupService);
        }

        public IEnumerable<IBuildTask> GetBuildTasks()
        {
            // remove Cosmos packages from the cache

            var xGlobalFolder = NuGet.Configuration.SettingsUtility.GetGlobalPackagesFolder(
                NuGet.Configuration.Settings.LoadDefaultSettings(Environment.SystemDirectory));

            void CleanPackage(string aPackage)
            {
                var xPath = Path.Combine(xGlobalFolder, aPackage);

                if (Directory.Exists(xPath))
                {
                    Directory.Delete(xPath, true);
                }
            }

            // Later we should specify the packages, currently we're moving to gen3 so package names are a bit unstable
            foreach (var xFolder in Directory.EnumerateDirectories(xGlobalFolder))
            {
                if (new DirectoryInfo(xFolder).Name.StartsWith("Cosmos", StringComparison.InvariantCultureIgnoreCase))
                {
                    CleanPackage(xFolder);
                }
            }

            var cosmosDir = Path.GetFullPath(Directory.GetCurrentDirectory());

            if (!Directory.Exists(Path.Combine(cosmosDir, "source", "Cosmos.Build.Builder")))
            {
                cosmosDir = Path.GetFullPath(Path.Combine(cosmosDir, "..", "..", "..", "..", ".."));
            }

            var il2cpuDir = Path.GetFullPath(Path.Combine(cosmosDir, "..", "IL2CPU"));
            var xsharpDir = Path.GetFullPath(Path.Combine(cosmosDir, "..", "XSharp"));

            var cosmosSourceDir = Path.Combine(cosmosDir, "source");
            var il2cpuSourceDir = Path.Combine(il2cpuDir, "source");

            var buildSlnPath = Path.Combine(cosmosDir, "Build.sln");
            var il2cpuSlnPath = Path.Combine(il2cpuDir, "IL2CPU.sln");
            var xsharpSlnPath = Path.Combine(xsharpDir, "XSharp.sln");

            var vsipDir = Path.Combine(cosmosDir, "Build", "VSIP");

            if (Directory.Exists(vsipDir))
            {
                Directory.Delete(vsipDir, true);
            }

            // Restore XSharp.sln, IL2CPU.sln and Build.sln

            yield return new RestoreTask(_msBuildService, xsharpSlnPath);
            yield return new RestoreTask(_msBuildService, il2cpuSlnPath);
            yield return new RestoreTask(_msBuildService, buildSlnPath);

            // Build Build.sln

            yield return new BuildTask(_msBuildService, buildSlnPath, vsipDir);

            // Publish IL2CPU

            var il2cpuProjectPath = Path.Combine(il2cpuSourceDir, "IL2CPU", "IL2CPU.csproj");
            var il2cpuPublishPath = Path.Combine(vsipDir, "IL2CPU");

            yield return new PublishTask(_msBuildService, il2cpuProjectPath, il2cpuPublishPath);

            // Pack build system and kernel assemblies

            var cosmosPackageProjects = new List<string>()
            {
                "Cosmos.Build.Tasks",

                "Cosmos.Common",

                "Cosmos.Core",
                "Cosmos.Core_Plugs",
                "Cosmos.Core_Asm",

                "Cosmos.HAL2",

                "Cosmos.System2",
                "Cosmos.System2_Plugs",

                "Cosmos.Debug.Kernel",
                "Cosmos.Debug.Kernel.Plugs.Asm"
            };

            var il2cpuPackageProjects = new List<string>()
            {
                "IL2CPU.API"
            };

            var packageProjectPaths = cosmosPackageProjects.Select(p => Path.Combine(cosmosSourceDir, p));
            packageProjectPaths = packageProjectPaths.Concat(il2cpuPackageProjects.Select(p => Path.Combine(il2cpuSourceDir, p)));

            var packagesDir = Path.Combine(vsipDir, "packages");

            foreach (var projectPath in packageProjectPaths)
            {
                yield return new PackTask(_msBuildService, projectPath, packagesDir);
            }

            var cosmosSetupDir = Path.Combine(cosmosDir, "setup");

            // Create Setup

            var innoSetupScriptPath = Path.Combine(cosmosSetupDir, "Cosmos.iss");
            var cosmosSetupVersion = App.BuilderConfiguration.UserKit ? DateTime.Now.ToString("yyyyMMdd") : "106027";

            yield return new CreateSetupTask(
                _innoSetupService,
                innoSetupScriptPath,
                App.BuilderConfiguration.UserKit ? "UserKit" : "DevKit",
                cosmosSetupVersion);

            var cosmosSetupPath = Path.Combine(cosmosSetupDir, "Output", $"CosmosUserKit-{cosmosSetupVersion}-vs2017.exe");

            // Run Setup

            yield return new StartProcessTask(cosmosSetupPath, "/SILENT", "Cosmos Setup", true);

            // Write Dev Kit path

            using (var xKey = Microsoft.Win32.Registry.CurrentUser.CreateSubKey(@"Software\Cosmos"))
            {
                xKey.SetValue("DevKit", cosmosDir);
            }

            // Launch VS

            if (!App.BuilderConfiguration.NoVsLaunch)
            {
                var vsInstance = _visualStudioInstance;
                var vsPath = Path.Combine(vsInstance.GetInstallationPath(), "Common7", "IDE", "devenv.exe");

                var kernelSlnPath = Path.Combine(cosmosDir, "Kernel.sln");

                yield return new StartProcessTask(vsPath, kernelSlnPath, "Visual Studio (Kernel.sln)");
            }
        }
    }
}
