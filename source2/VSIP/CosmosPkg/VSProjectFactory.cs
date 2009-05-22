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
      : base(package) {
      this.package = package;
    }

    protected override ProjectNode CreateProject() {
      VSProjectNode project = new VSProjectNode(this.package);

      project.SetSite((IOleServiceProvider)((IServiceProvider)this.package).GetService(typeof(IOleServiceProvider)));
      return project;
    }
  }
}
