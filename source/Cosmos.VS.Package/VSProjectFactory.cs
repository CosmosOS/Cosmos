using Microsoft.VisualStudio.Project;
using System;
using System.Runtime.InteropServices;
using IOleServiceProvider = Microsoft.VisualStudio.OLE.Interop.IServiceProvider;

namespace Cosmos.VS.Package
{
    [Guid(Guids.guidProjectFactoryString)]
    internal class VSProjectFactory : ProjectFactory
    {
        public VSProjectFactory(VSProject package)
            : base(package)
        {
        }

        protected override ProjectNode CreateProject()
        {
            try
            {
                var project = new VSProjectNode(this.Package as VSProject);

                project.SetSite((IOleServiceProvider)((IServiceProvider)this.Package).GetService(typeof(IOleServiceProvider)));
                LogUtility.LogString("(Result == null) = {0}", project == null);
                return project;
            }
            catch (Exception E)
            {
                LogUtility.LogException(E);
                throw;
            }
        }
    }
}