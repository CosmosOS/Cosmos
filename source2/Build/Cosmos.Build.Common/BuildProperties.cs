using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.Build.Common {
  public class BuildProperties : PropertiesBase {

    // Profile
    public const string ProfileString = "Profile";
    public Profile Profile {
      get { return GetProperty(ProfileString, Profile.VMware); }
      set { SetProperty(ProfileString, value); }
    }

    // Deployment
    public const string DeploymentString = "Deployment";
    public Deployment Deployment {
      get { return GetProperty(DeploymentString, Deployment.ISO); }
      set { SetProperty(DeploymentString, value); }
    }

    // Launch
    public const string LaunchString = "Launch";
    public Launch Launch {
      get { return GetProperty(LaunchString, Launch.VMware); }
      set { SetProperty(LaunchString, value); }
    }

    // Debug
    public const string DebugEnabledString = "DebugEnabled";
    public bool DebugEnabled {
      get { return GetProperty(DebugEnabledString, true); }
      set { SetProperty(DebugEnabledString, value); }
    }
    public const string DebugModeString = "DebugMode";
    public DebugMode DebugMode {
      get { return GetProperty(DebugModeString, DebugMode.Source); }
      set { SetProperty(DebugModeString, value); }
    }
    public const string IgnoreDebugStubAttributeString = "IgnoreDebugStubAttribute";
    public bool IgnoreDebugStubAttribute {
      get { return GetProperty(IgnoreDebugStubAttributeString, false); }
      set { SetProperty(IgnoreDebugStubAttributeString, value); }
    }

    // VMware
    public const string VMwareEditionString = "VMwareEdition";
    public VMwareEdition VMwareEdition {
      get { return GetProperty(VMwareEditionString, VMwareEdition.Player); }
      set { SetProperty(VMwareEditionString, value); }
    }

    public const string OutputPathString = "OutputPath";
    public String OutputPath {
      get { return GetProperty(OutputPathString); }
      set { SetProperty(OutputPathString, value); }
    }
    public const string FrameworkString = "Framework";
    public Framework Framework {
      get { return GetProperty(FrameworkString, Common.Framework.MicrosoftNET); }
      set { SetProperty(FrameworkString, value); }
    }
    public const string UseInternalAssemblerString = "UseInternalAssembler";
    public Boolean UseInternalAssembler {
      get { return GetProperty(UseInternalAssemblerString, false); }
      set { SetProperty(UseInternalAssemblerString, value); }
    }

    public const string TraceAssembliesString = "TraceAssemblies";
    public TraceAssemblies TraceAssemblies {
      get { return GetProperty(TraceAssembliesString, TraceAssemblies.User); }
      set { SetProperty(TraceAssembliesString, value); }
    }

    public const string EnableGDBString = "EnableGDB";
    public Boolean EnableGDB {
      get { return GetProperty(EnableGDBString, false); }
      set { SetProperty(EnableGDBString, value); }
    }
    public const string StartCosmosGDBString = "StartCosmosGDB";
    public bool StartCosmosGDB {
      get { return GetProperty(StartCosmosGDBString, false); }
      set { SetProperty(StartCosmosGDBString, value); }
    }
  }
}