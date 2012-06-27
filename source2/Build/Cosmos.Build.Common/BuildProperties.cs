using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.Build.Common {
  public class BuildProperties : PropertiesBase {

    protected String GetProfileProperty(string aName) {
      return GetProperty(Prefix + aName); 
    }
    protected T GetProfileProperty<T>(string aName, T aDefault) {
      return GetProperty(Prefix + aName, aDefault);
    }

    public void SetProfileProperty(string aName, object aValue) {
      SetProperty(Prefix + aName, aValue);
    }

    protected string Prefix {
      get {
        return "";
        //return Profile + "_";
      }
    }

    // Profile
    public Profile Profile {
      get { return GetProperty("Profile", Profile.VMware); }
      set { SetProperty("Profile", value); }
    }

    // Deployment
    public Deployment Deployment {
      get { return GetProfileProperty("Deployment", Deployment.ISO); }
      set { SetProfileProperty("Deployment", value); }
    }

    // Launch
    public Launch Launch {
      get { return GetProfileProperty("Launch", Launch.VMware); }
      set { SetProfileProperty("Launch", value); }
    }

    // VMware
    public VMwareEdition VMwareEdition {
      get { return GetProfileProperty("VMwareEdition", VMwareEdition.Player); }
      set { SetProfileProperty("VMwareEdition", value); }
    }

    public String OutputPath {
      get { return GetProfileProperty("OutputPath"); }
      set { SetProfileProperty("OutputPath", value); }
    }
    public Framework Framework {
      get { return GetProfileProperty("Framework", Common.Framework.MicrosoftNET); }
      set { SetProfileProperty("Framework", value); }
    }
    public Boolean UseInternalAssembler {
      get { return GetProfileProperty("UseInternalAssembler", false); }
      set { SetProfileProperty("UseInternalAssembler", value); }
    }

    public TraceAssemblies TraceAssemblies {
      get { return GetProfileProperty("TraceAssemblies", TraceAssemblies.User); }
      set { SetProfileProperty("TraceAssemblies", value); }
    }

    public DebugMode DebugMode {
      get { return GetProfileProperty("DebugMode", DebugMode.Source); }
      set { SetProfileProperty("DebugMode", value); }
    }
    public bool IgnoreDebugStubAttribute {
      get { return GetProfileProperty("IgnoreDebugStubAttribute", false); }
      set { SetProfileProperty("IgnoreDebugStubAttribute", value); }
    }
    public Boolean EnableGDB {
      get { return GetProfileProperty("EnableGDB", false); }
      set { SetProfileProperty("EnableGDB", value); }
    }
    public bool StartCosmosGDB {
      get { return GetProfileProperty("StartCosmosGDB", false); }
      set { SetProfileProperty("StartCosmosGDB", value); }
    }
  }
}