using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.Project;

namespace Cosmos.VS.Package {
  public class VsConfigProvider : ConfigProvider {
	
    public VsConfigProvider(ProjectNode manager) : base(manager) {}
  
  }
}
