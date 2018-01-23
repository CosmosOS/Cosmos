using System;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

using Cosmos.VS.ProjectSystem.VS.PropertyPages;

namespace Cosmos.VS.ProjectSystem
{
    [Guid(PackageGuid)]
    [PackageRegistration(UseManagedResourcesOnly = true)]
    [ProvideObject(typeof(CosmosPropertyPage))]
    [ProvideProjectFactory(typeof(MigrateCosmosProjectFactory), null, "Cosmos Project Files (*.Cosmos);*.Cosmos", "Cosmos", "Cosmos", null)]
    internal class CosmosProjectSystemPackage : Package
    {
        /// <summary>
        /// The GUID for this package.
        /// </summary>
        public const string PackageGuid = "29194faf-90ce-454b-bc53-08b722f1dadf";

        private IVsProjectFactory _factory;

        protected override void Initialize()
        {
            base.Initialize();

            _factory = new MigrateCosmosProjectFactory();
            RegisterProjectFactory(_factory);
        }
    }
}
