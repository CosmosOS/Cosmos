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
        public bool ShouldInstallByDefault => false;
        public string Name => "Visual Studio Workloads";

        public string OtherDependencysThatAreMissing
        {
            get
            {
                var missingPackages = ((string[])RequiredPackages.Clone()).ToList();
                foreach (var item in RequiredPackages)
                {
                    if (IsPackageInstalled(item))
                    {
                        missingPackages.Remove(item);
                    }
                }

                //Add the missing packages together
                string missingPackages_proper = "";
                foreach (var item in missingPackages)
                {
                    missingPackages_proper += GetProperName(item) + ", ";
                }

                return missingPackages_proper;
            }
        }

        private readonly ISetupInstance2 _visualStudioInstance;

        public VisualStudioWorkloadsDependency(ISetupInstance2 visualStudioInstance)
        {
            _visualStudioInstance = visualStudioInstance;
        }

        public Task<bool> IsInstalledAsync(CancellationToken cancellationToken)
        {
            var installedPackages = _visualStudioInstance.GetPackages();
            return Task.FromResult(RequiredPackages.All(p => IsPackageInstalled(p)));
        }
        private string GetProperName(string packageId)
        {
            if (packageId == NetCoreToolsWorkload)
            {
                return ".NET Core cross-platform development";
            }
            else if (packageId == VisualStudioExtensionsWorkload)
            {
                return "Visual Studio Extension development";
            }

            return "Unknown Workload: " + packageId;
        }
        public async Task InstallAsync(CancellationToken cancellationToken)
        {
            var vsInstallerPath = Environment.ExpandEnvironmentVariables(
                @"%ProgramFiles(x86)%\Microsoft Visual Studio\Installer\vs_installershell.exe");

            var vsInstancePath = _visualStudioInstance.GetInstallationPath();
            var installedPackages = _visualStudioInstance.GetPackages();

            var args = $"modify --passive --norestart --installPath \"{vsInstancePath}\"";

            foreach (var workload in RequiredPackages)
            {
                if (!IsPackageInstalled(workload))
                {
                    args += $" --add {workload}";
                }
            }

            var process = Process.Start(vsInstallerPath, args);
            await Task.Run(process.WaitForExit, cancellationToken).ConfigureAwait(false);

            if (process.ExitCode != 0)
            {
                throw new Exception("The process failed to execute!");
            }
        }

        private bool IsPackageInstalled(string packageId) =>
            _visualStudioInstance.GetPackages().Any(
                p => String.Equals(p.GetId(), packageId, StringComparison.Ordinal));
    }
}
