using System;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Shell;

using Cosmos.VS.ProjectSystem.VS.PropertyPages;

namespace Cosmos.VS.ProjectSystem
{
    [Guid(PackageGuid)]
    [PackageRegistration(UseManagedResourcesOnly = true)]
    [ProvideObject(typeof(CosmosPropertyPage))]
    internal class CosmosProjectSystemPackage : Package
    {
        /// <summary>
        /// The GUID for this package.
        /// </summary>
        public const string PackageGuid = "29194faf-90ce-454b-bc53-08b722f1dadf";
    }
}
