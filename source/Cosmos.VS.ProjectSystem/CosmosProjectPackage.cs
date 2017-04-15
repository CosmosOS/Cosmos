using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.InteropServices;
using Cosmos.VS.ProjectSystem.PropertyPages;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Project;

namespace Cosmos.VS.ProjectSystem
{
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "pkgdef, VS and vsixmanifest are valid VS terms")]
    [PackageRegistration(UseManagedResourcesOnly = true)]
    [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)]
    [ProvideProjectFactory(typeof(CosmosProjectFactory), null, "Cosmos Project Files (*.Cosmos);*.Cosmos", "Cosmos", "Cosmos", @".\NullPath", LanguageVsTemplate = "Cosmos")]
    [ProvideObject(typeof(CosmosPage), RegisterUsing = RegistrationMethod.CodeBase)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [Guid(Guids.guidCosmosProjectPkgString)]
    public sealed class CosmosProjectPackage : ProjectPackage
    {
        public CosmosProjectPackage()
        {
            //Logger.LogEvent += Global.OutputPane.OutputString;
            Logger.LogEvent += aMessage => Trace.WriteLine(aMessage);
            Logger.TraceMethod(MethodBase.GetCurrentMethod());
        }

        #region Package Members

        protected override void Initialize()
        {
            Logger.TraceMethod(MethodBase.GetCurrentMethod());

            base.Initialize();
            this.RegisterProjectFactory(new CosmosProjectFactory(this));
        }

        public override string ProductUserContext => "";

        #endregion
    }
}