using System;
using System.Reflection;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Project;
using IOleServiceProvider = Microsoft.VisualStudio.OLE.Interop.IServiceProvider;

namespace Cosmos.VS.ProjectSystem
{
    [Guid(Guids.guidCosmosProjectFactoryString)]
    class CosmosProjectFactory : ProjectFactory
    {
        private readonly CosmosProjectPackage mPackage;

        public CosmosProjectFactory(CosmosProjectPackage aPackage)
            : base(aPackage)
        {
            Logger.TraceMethod(MethodBase.GetCurrentMethod());

            mPackage = aPackage;
        }

        protected override ProjectNode CreateProject()
        {
            Logger.TraceMethod(MethodBase.GetCurrentMethod());

            var xProject = new CosmosProjectNode(mPackage);
            xProject.SetSite((IOleServiceProvider)((IServiceProvider)mPackage).GetService(typeof(IOleServiceProvider)));
            return xProject;
        }
    }
}