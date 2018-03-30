using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Setup.Configuration;

namespace Cosmos.Build.Builder.Dependencies
{
    internal class VisualStudioWorkloadsDependency : IDependency
    {
        private const string NetCoreToolsWorkload = "Microsoft.VisualStudio.Workload.NetCoreTools";
        private const string VisualStudioExtensionsWorkload = "Microsoft.VisualStudio.Workload.VisualStudioExtension";

        private static readonly string[] RequiredPackages = new string[]
        {
            NetCoreToolsWorkload,
            VisualStudioExtensionsWorkload
        };

        public string Name => "Visual Studio Workloads";

        private ISetupInstance2 _visualStudioInstance;

        public VisualStudioWorkloadsDependency(ISetupInstance2 visualStudioInstance)
        {
            _visualStudioInstance = visualStudioInstance;
        }

        public Task<bool> IsInstalledAsync(CancellationToken cancellationToken)
        {
            var installedPackages = _visualStudioInstance.GetPackages();
            return Task.FromResult(RequiredPackages.All(p => IsPackageInstalled(p)));
        }

        public Task InstallAsync(CancellationToken cancellationToken)
        {
            var vsInstallerPath = Environment.ExpandEnvironmentVariables(
                @"%ProgramFiles(x86)%\Microsoft Visual Studio\Installer\vs_installershell.exe");

            var vsInstancePath = _visualStudioInstance.GetInstallationPath();
            var installedPackages = _visualStudioInstance.GetPackages();

            var args = $"modify --passive --norestart --wait --installPath \"{vsInstancePath}\"";

            foreach (var workload in RequiredPackages)
            {
                if (!IsPackageInstalled(workload))
                {
                    args += $" --add {workload}";
                }
            }

            var process = Process.Start(vsInstallerPath, args);
            return Task.Run((Action)process.WaitForExit, cancellationToken);
        }

        private bool IsPackageInstalled(string packageId) =>
            _visualStudioInstance.GetPackages().Any(p => p.GetId() == packageId);
    }
}
