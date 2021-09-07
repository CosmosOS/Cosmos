using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace Cosmos.Build.Builder.Dependencies
{
    internal class ProperRepoNameDependency : IDependency
    {
        public string Name => "Proper Cosmos folder name";

        public bool ShouldInstallByDefault => false;

        public string OtherDependencysThatAreMissing => "rename the directory from where install-VS2019.bat or userkit install.bat is started to Cosmos.";
        private string CosmosDir;

        public ProperRepoNameDependency(string CosmosDir)
        {
            this.CosmosDir = CosmosDir;
        }
        public Task InstallAsync(CancellationToken cancellationToken) { throw new NotImplementedException("Installing Proper Cosmos Repository name is not supported"); }
        public Task<bool> IsInstalledAsync(CancellationToken cancellationToken)
        {
            var topDir = CosmosDir.Replace(Path.GetDirectoryName(CosmosDir) + Path.DirectorySeparatorChar, "");
            if (topDir.ToLower() != "cosmos")
            {
                return Task.FromResult(false);
            }
            else
            {
                return Task.FromResult(true);
            }
        }
    }
}
