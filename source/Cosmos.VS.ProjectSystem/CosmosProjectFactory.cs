using System;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Project;
using Microsoft.VisualStudio.Shell;
using IOleServiceProvider = Microsoft.VisualStudio.OLE.Interop.IServiceProvider;

namespace Cosmos.VS.ProjectSystem
{
    [Guid(Guids.guidCosmosProjectFactoryString)]
    class CosmosProjectFactory : ProjectFactory
    {
        private CosmosProjectPackage package;

        public CosmosProjectFactory(CosmosProjectPackage package)
            : base(package)
        {
            this.package = package;
        }

        protected override ProjectNode CreateProject()
        {
            CosmosProjectNode project = new CosmosProjectNode(this.package);

            project.SetSite((IOleServiceProvider)((IServiceProvider)this.package).GetService(typeof(IOleServiceProvider)));
            return project;
        }
    }
}