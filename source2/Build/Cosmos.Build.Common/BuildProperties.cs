using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.Build.Common {
  public class BuildProperties : PropertiesBase {

    // Execute
    public BuildTarget BuildTarget {
      get { return this.GetProperty("BuildTarget", BuildTarget.VMware); }
      set { this.SetProperty("BuildTarget", value); }
    }

    // VMware
    public VMwareEdition VMwareEdition {
      get { return GetProperty("VMwareEdition", VMwareEdition.Player); }
      set { SetProperty("VMwareEdition", value); }
    }
    public VMwareDeploy VMwareDeploy {
      get { return GetProperty("VMwareDeploy", VMwareDeploy.ISO); }
      set { SetProperty("VMwareDeploy", value); }
    }

    public String OutputPath {
      get { return this.GetProperty("OutputPath"); }
      set { this.SetProperty("OutputPath", value); }
    }
    public Framework Framework {
      get { return this.GetProperty("Framework", Common.Framework.MicrosoftNET); }
      set { this.SetProperty("Framework", value); }
    }
    public Boolean UseInternalAssembler {
      get { return this.GetProperty("UseInternalAssembler", false); }
      set { this.SetProperty("UseInternalAssembler", value); }
    }

    public TraceAssemblies TraceAssemblies {
      get { return GetProperty("TraceAssemblies", TraceAssemblies.User); }
      set { SetProperty("TraceAssemblies", value); }
    }

    public DebugMode DebugMode {
      get { return GetProperty("DebugMode", DebugMode.Source); }
      set { SetProperty("DebugMode", value); }
    }
    public bool IgnoreDebugStubAttribute {
      get { return GetProperty("IgnoreDebugStubAttribute", false); }
      set { SetProperty("IgnoreDebugStubAttribute", value); }
    }
    public Boolean EnableGDB {
      get { return GetProperty("EnableGDB", false); }
      set { SetProperty("EnableGDB", value); }
    }
    public bool StartCosmosGDB {
      get { return GetProperty("StartCosmosGDB", false); }
      set { SetProperty("StartCosmosGDB", value); }
    }
  }
}