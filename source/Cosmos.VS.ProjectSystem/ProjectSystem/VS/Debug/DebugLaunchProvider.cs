using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.ProjectSystem;
using Microsoft.VisualStudio.ProjectSystem.Debug;
using Microsoft.VisualStudio.ProjectSystem.VS.Debug;
using Microsoft.VisualStudio.Threading;

using Cosmos.Build.Common;

using static Cosmos.VS.ProjectSystem.LaunchConfiguration;

namespace Cosmos.VS.ProjectSystem.VS.Debug
{
    [ExportDebugger(CosmosDebugger.SchemaName)]
    [AppliesTo(ProjectCapability.Cosmos)]
    internal class DebugLaunchProvider : DebugLaunchProviderBase
    {
        private static readonly Guid CosmosDebugEngineGuid = new Guid("fa1da3a6-66ff-4c65-b077-e65f7164ef83");

        private readonly ProjectProperties _projectProperties;
        private readonly IBootableProperties _bootableProperties;

        private readonly IProjectLockService _projectLockService;

        [ImportingConstructor]
        public DebugLaunchProvider(
            ConfiguredProject configuredProject,
            ProjectProperties projectProperties,
            IBootableProperties bootableProperties,
            IProjectLockService projectLockService)
            : base(configuredProject)
        {
            _projectProperties = projectProperties;
            _bootableProperties = bootableProperties;

            _projectLockService = projectLockService;
        }

        public override Task<bool> CanLaunchAsync(DebugLaunchOptions aLaunchOptions) => TplExtensions.TrueTask;

        public override async Task<IReadOnlyList<IDebugLaunchSettings>> QueryDebugTargetsAsync(DebugLaunchOptions aLaunchOptions)
        {
            var launchConfiguration = await _projectProperties.GetLaunchConfigurationPropertiesAsync();
            var deploymentType = await launchConfiguration.Deployment.GetEvaluatedValueAtEndAsync();
            var launchType = await launchConfiguration.Launch.GetEvaluatedValueAtEndAsync();
            var debugPort = await launchConfiguration.VisualStudioDebugPort.GetEvaluatedValueAtEndAsync();

            var isoFile = await _bootableProperties.GetIsoFileAsync();
            var binFile = await _bootableProperties.GetBinFileAsync();

            if (deploymentType == DeploymentValues.ISO)
            {
                // ISO is created by the MakeIso target
            }
            else if (deploymentType == DeploymentValues.USB)
            {
                Process.Start(Path.Combine(CosmosPaths.Tools, "Cosmos.Deploy.USB.exe"), "\"" + binFile + "\"");

            }
            else if (deploymentType == DeploymentValues.PXE)
            {
                string pxePath = Path.Combine(CosmosPaths.Build, "PXE");
                string pxeIntf = await GetPropertyAsync(BuildPropertyNames.PxeInterfaceString);
                File.Copy(binFile, Path.Combine(pxePath, "Cosmos.bin"), true);
                Process.Start(Path.Combine(CosmosPaths.Tools, "Cosmos.Deploy.Pixie.exe"), pxeIntf + " \"" + pxePath + "\"");
            }
            else if (deploymentType == DeploymentValues.BinaryImage)
            {
                // prepare?
            }
            else
            {
                throw new Exception("Unknown deployment type.");
            }

            if (launchType == LaunchValues.None
                && deploymentType == DeploymentValues.ISO)
            {
                Process.Start(Path.GetDirectoryName(isoFile));
            }
            else
            {
                var debugLaunchSettings = new DebugLaunchSettings(aLaunchOptions)
                {
                    LaunchOperation = DebugLaunchOperation.CreateProcess,
                    LaunchDebugEngineGuid = CosmosDebugEngineGuid,
                    CurrentDirectory = Path.GetDirectoryName(ConfiguredProject.UnconfiguredProject.FullPath)
                };

                var values = new Dictionary<string, string>
                {
                    ["ProjectFile"] = ConfiguredProject.UnconfiguredProject.FullPath,
                    ["ISOFile"] = ConfiguredProject.UnconfiguredProject.MakeRooted(isoFile),
                    ["BinFormat"] = await GetPropertyAsync("BinFormat"),
                    ["OutputPath"] = await GetPropertyAsync("OutputPath")
                };

                foreach (var xName in BuildProperties.PropNames)
                {
                    values[xName] = await GetPropertyAsync(xName);
                }

                debugLaunchSettings.Executable = String.Join(";", values.Select(v => $"{v.Key}={v.Value}"));

                return new DebugLaunchSettings[] { debugLaunchSettings };
            }

            return Array.Empty<IDebugLaunchSettings>();
        }
        
        private async Task<string> GetPropertyAsync(string propertyName)
        {
            using (var projectReadLock = await _projectLockService.ReadLockAsync())
            {
                var project = await projectReadLock.GetProjectAsync(ConfiguredProject);
                return project.GetPropertyValue(propertyName);
            }
        }
    }
}
