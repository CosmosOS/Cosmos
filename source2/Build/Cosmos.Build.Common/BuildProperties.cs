using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.Build.Common {
  public class BuildProperties : PropertiesBase {
    static public List<string> PropNames = new List<string>();

    static BuildProperties() {
      foreach (var xField in typeof(BuildProperties).GetFields()) {
        // IsLiteral determines if its value is written at compile time and not changeable.
        // Consts are static even if we dont use static keyword.
        // IsInitOnly determine if the field can be set in the body of the constructor
        // for C# a field which is readonly keyword would have both true but a const field would have only IsLiteral equal to true
        if (xField.IsLiteral && !xField.IsInitOnly && xField.FieldType == typeof(string)) {
          string xName = (string)xField.GetValue(null);
          if (xName != BuildProperties.ProfileString) {
            PropNames.Add(xName);
          }
        }
      }
    }

    public void DeleteProfile(string aPrefix) {
      foreach (var xName in BuildProperties.PropNames) {
        mPropTable.Remove(aPrefix + "_" + xName);
      }
    }

    public const string ProfileString = "Profile";
    public string Profile {
      get { return GetProperty(ProfileString, "VMware"); }
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
      get { return GetProperty(OutputPathString, @"bin\debug"); }
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