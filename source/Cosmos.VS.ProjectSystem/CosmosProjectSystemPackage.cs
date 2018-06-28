using System;
using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Task = System.Threading.Tasks.Task;

using Cosmos.VS.ProjectSystem.VS.PropertyPages;

namespace Cosmos.VS.ProjectSystem
{
    [Guid(PackageGuid)]
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [ProvideObject(typeof(OldCosmosPropertyPage))]
    [ProvideObject(typeof(CosmosPropertyPage))]
    [ProvideProjectFactory(typeof(MigrateCosmosProjectFactory), null, "Cosmos Project Files (*.Cosmos);*.Cosmos", "Cosmos", "Cosmos", null)]
    internal class CosmosProjectSystemPackage : AsyncPackage
    {
        /// <summary>
        /// The GUID for this package.
        /// </summary>
        public const string PackageGuid = "29194faf-90ce-454b-bc53-08b722f1dadf";

        private IVsProjectFactory _factory;

        protected override async Task InitializeAsync(
            CancellationToken cancellationToken,
            IProgress<ServiceProgressData> progress)
        {
            await base.InitializeAsync(cancellationToken, progress).ConfigureAwait(false);

            await JoinableTaskFactory.SwitchToMainThreadAsync();

            _factory = new MigrateCosmosProjectFactory();
            RegisterProjectFactory(_factory);
        }
    }
}
