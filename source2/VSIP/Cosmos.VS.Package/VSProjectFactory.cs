using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Project;
using IOleServiceProvider = Microsoft.VisualStudio.OLE.Interop.IServiceProvider;

namespace Cosmos.VS.Package {
  [Guid(Guids.guidProjectFactoryString)]
  class VSProjectFactory : ProjectFactory {
    private VSProject package;

    public VSProjectFactory(VSProject package)
        : base(package)
    {
        LogUtility.LogString("Entering Cosmos.VS.Package.VSProjectFactory.ctor(VSProject)");
        this.package = package;
        LogUtility.LogString("Exiting Cosmos.VS.Package.VSProjectFactory.ctor(VSProject)");
    }

    protected override ProjectNode CreateProject() {
        LogUtility.LogString("Entering Cosmos.VS.Package.VSProjectFactory.CreateProject()");
        try
        {
            VSProjectNode project = new VSProjectNode(this.package);

            project.SetSite((IOleServiceProvider)((IServiceProvider)this.package).GetService(typeof(IOleServiceProvider)));
            LogUtility.LogString("(Result == null) = {0}", project == null);
            return project;
        }
        catch (Exception E)
        {
            LogUtility.LogException(E);
            throw;
        }
        finally
        {
            LogUtility.LogString("Exiting Cosmos.VS.Package.VSProjectFactory.CreateProject()");
        }
    }
  }
}
