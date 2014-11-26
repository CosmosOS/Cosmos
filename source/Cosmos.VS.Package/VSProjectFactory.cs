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

    public VSProjectFactory(VSProject package)
      : base(package) {
    }

    protected override ProjectNode CreateProject() {
      try {
        var project = new VSProjectNode(this.Package as VSProject);

        project.SetSite((IOleServiceProvider)((IServiceProvider)this.Package).GetService(typeof(IOleServiceProvider)));
        LogUtility.LogString("(Result == null) = {0}", project == null);
        return project;
      } catch (Exception E) {
        LogUtility.LogException(E);
        throw;
      }
    }
  }
}