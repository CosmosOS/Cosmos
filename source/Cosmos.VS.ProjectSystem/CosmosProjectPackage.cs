using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Project;

namespace Cosmos.VS.ProjectSystem
{
    [SuppressMessage("StyleCop.CSharp.DocumentationRules",
        "SA1650:ElementDocumentationMustBeSpelledCorrectly",
        Justification = "pkgdef, VS and vsixmanifest are valid VS terms")]
    [PackageRegistration(UseManagedResourcesOnly = true)]
    [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)]
    [ProvideProjectFactory(typeof(CosmosProjectFactory), null, "Cosmos Project Files (*.Cosmos);*.Cosmos", "Cosmos", "Cosmos", @".\NullPath", LanguageVsTemplate = "Cosmos")]
    [Guid(Guids.guidCosmosProjectPkgString)]
    public sealed class CosmosProjectPackage : ProjectPackage
    {
        public CosmosProjectPackage()
        {

        }

        #region Package Members

        protected override void Initialize()
        {
            base.Initialize();
            this.RegisterProjectFactory(new CosmosProjectFactory(this));
        }

        public override string ProductUserContext => "";

        #endregion
    }
}