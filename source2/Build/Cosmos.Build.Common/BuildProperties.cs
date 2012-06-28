using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.Build.Common {
  public class BuildProperties : PropertiesBase {

    // Profile
    public Profile Profile {
      get { return GetProperty("Profile", Profile.VMware); }
      set { SetProperty("Profile", value); }
    }

    // Deployment
    public Deployment Deployment {
      get { return GetProperty("Deployment", Deployment.ISO); }
      set { SetProperty("Deployment", value); }
    }

    // Launch
    public Launch Launch {
      get { return GetProperty("Launch", Launch.VMware); }
      set { SetProperty("Launch", value); }
    }

    // Debug
    public bool DebugEnabled {
      get { return GetProperty("DebugEnabled", true); }
      set { SetProperty("DebugEnabled", value); }
    }
    public DebugMode DebugMode {
      get { return GetProperty("DebugMode", DebugMode.Source); }
      set { SetProperty("DebugMode", value); }
    }
    public bool IgnoreDebugStubAttribute {
      get { return GetProperty("IgnoreDebugStubAttribute", false); }
      set { SetProperty("IgnoreDebugStubAttribute", value); }
    }

    // VMware
    public VMwareEdition VMwareEdition {
      get { return GetProperty("VMwareEdition", VMwareEdition.Player); }
      set { SetProperty("VMwareEdition", value); }
    }

    public String OutputPath {
      get { return GetProperty("OutputPath"); }
      set { SetProperty("OutputPath", value); }
    }
    public Framework Framework {
      get { return GetProperty("Framework", Common.Framework.MicrosoftNET); }
      set { SetProperty("Framework", value); }
    }
    public Boolean UseInternalAssembler {
      get { return GetProperty("UseInternalAssembler", false); }
      set { SetProperty("UseInternalAssembler", value); }
    }

    public TraceAssemblies TraceAssemblies {
      get { return GetProperty("TraceAssemblies", TraceAssemblies.User); }
      set { SetProperty("TraceAssemblies", value); }
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