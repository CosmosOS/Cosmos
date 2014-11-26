using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.Project;

namespace Cosmos.VS.Package {
  public class VsConfigProvider : ConfigProvider {

    public VsConfigProvider(ProjectNode manager) : base(manager) { }

    public override int GetCfgNames(uint celt, string[] names, uint[] actual) {
      return base.GetCfgNames(celt, names, actual);
    }

    protected override ProjectConfig CreateProjectConfiguration(string configName) {
      try {
        return new VsProjectConfig(ProjectMgr, configName);
      } finally {
      }
    }

  }
}
