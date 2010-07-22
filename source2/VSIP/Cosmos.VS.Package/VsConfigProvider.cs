using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.Project;

namespace Cosmos.VS.Package {
  public class VsConfigProvider : ConfigProvider {
	
    public VsConfigProvider(ProjectNode manager) : base(manager) {}

    protected override ProjectConfig CreateProjectConfiguration(string configName) {
        LogUtility.LogString("Entering Cosmos.VS.Package.VsConfigProvider.CreateProjectConfiguration('{0}')", configName);
        try
        {
            return new VsProjectConfig(ProjectMgr, configName);
        }finally{
            LogUtility.LogString("Exiting Cosmos.VS.Package.VsConfigProvider.CreateProjectConfiguration(string)");
        }
    }
  
  }
}
  