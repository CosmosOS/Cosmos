using System;
using System.Collections.Generic;
using System.IO;
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

    public void SaveProfile(string aName) {
      foreach (var xName in BuildProperties.PropNames) {
        string xValue = GetProperty(xName);
        if (!string.IsNullOrWhiteSpace(xValue)) {
          SetProperty(aName + "_" + xName, xValue);
        }
      }
    }

    public void LoadProfile(string aName) {
      foreach (var xName in BuildProperties.PropNames) {
        string xValue = GetProperty(aName + "_" + xName);
        if (!string.IsNullOrWhiteSpace(xValue)) {
          SetProperty(xName, xValue);
        }
      }

      // Reforce fixed settings for presets on each load.
      if (aName == "ISO") {
        Description = "Creates a bootable ISO image which can be burned to a DVD."
         + " After running the selected project, an explorer window will open containing the ISO file."
         + " The ISO file can then be burned to a CD or DVD and used to boot a physical or virtual system.";
        Deployment = DeploymentType.ISO;
        Launch = LaunchType.None;

      } else if (aName == "USB") {
        Description = "Makes a USB device such as a flash drive or external hard disk bootable.";
        Deployment = DeploymentType.USB;
        Launch = LaunchType.None;

      } else if (aName == "VMware") {
        Description = "Use VMware Player or Workstation to deploy and debug.";
        Deployment = DeploymentType.ISO;
        Launch = LaunchType.VMware;

      } else if (aName == "PXE") {
        Description = "Creates a PXE setup and hosts a DCHP and TFTP server to deploy directly to physical hardware. Allows debugging with a serial cable.";
        Deployment = DeploymentType.PXE;
        Launch = LaunchType.None;

      } else if (aName == "Bochs") {
        Description = "Use Bochs emulator to deploy and debug.";
        Deployment = DeploymentType.ISO;
        Launch = LaunchType.Bochs;
      }
      else if (aName == "IntelEdison")
      {
        Description = "Connect to Intel Edison device to deploy and debug.";
        Deployment = DeploymentType.BinaryImage;
        Launch = LaunchType.IntelEdison;
      }
    }

    public void DeleteProfile(string aPrefix) {
      foreach (var xName in BuildProperties.PropNames) {
        mPropTable.Remove(aPrefix + "_" + xName);
      }
    }

    // Profile
    public const string ProfileString = "Profile";
    public string Profile {
      get { return GetProperty(ProfileString, "VMware"); }
      set { SetProperty(ProfileString, value); }
    }
    public const string NameString = "Name";
    public string Name {
      get { return GetProperty(NameString, ""); }
      set { SetProperty(NameString, value); }
    }
    public const string DescriptionString = "Description";
    public string Description {
      get { return GetProperty(DescriptionString, ""); }
      set { SetProperty(DescriptionString, value); }
    }

    // Deployment
    public const string DeploymentString = "Deployment";
    public DeploymentType Deployment {
      get { return GetProperty(DeploymentString, DeploymentType.ISO); }
      set { SetProperty(DeploymentString, value); }
    }

    // Launch
    public const string LaunchString = "Launch";
    public LaunchType Launch {
      get { return GetProperty(LaunchString, LaunchType.VMware); }
      set { SetProperty(LaunchString, value); }
    }
    public const string ShowLaunchConsoleString = "ShowLaunchConsole";
    public bool ShowLaunchConsole {
      get { return GetProperty(ShowLaunchConsoleString, false); }
      set { SetProperty(ShowLaunchConsoleString, value); }
    }

    // Debug
    public const string DebugEnabledString = "DebugEnabled";
    public bool DebugEnabled {
      get { return GetProperty(DebugEnabledString, true); }
      set { SetProperty(DebugEnabledString, value); }
    }
    public const string StackCorruptionDetectionEnabledString = "StackCorruptionDetectionEnabled";
    public bool StackCorruptionDetectionEnabled
    {
      get { return GetProperty(StackCorruptionDetectionEnabledString, true); }
      set { SetProperty(StackCorruptionDetectionEnabledString, value); }
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
    public const string CosmosDebugPortString = "CosmosDebugPort";
    public string CosmosDebugPort {
      get { return GetProperty(CosmosDebugPortString, "Serial: COM1"); }
      set { SetProperty(CosmosDebugPortString, value); }
    }
    public const string VisualStudioDebugPortString = "VisualStudioDebugPort";
    public string VisualStudioDebugPort {
      get { return GetProperty(VisualStudioDebugPortString, "Serial: COM1"); }
      set { SetProperty(VisualStudioDebugPortString, value); }
    }

    // PXE
    public const string PxeInterfaceString = "PxeInterface";
    public string PxeInterface {
      get { return GetProperty(PxeInterfaceString, "192.168.42.1"); }
      set { SetProperty(PxeInterfaceString, value); }
    }
    public const string SlavePortString = "SlavePort";
    public string SlavePort {
      get { return GetProperty(SlavePortString, "None"); }
      set { SetProperty(SlavePortString, value); }
    }

    // Bochs
    public const string BochsDefaultConfigurationFileName = "Cosmos.bxrc";
    public const string BochsEmulatorConfigurationFileString = "BochsConfig";
    public string BochsEmulatorConfigurationFile
    {
      get {
        return GetProperty(BochsEmulatorConfigurationFileString,
          Path.Combine(OutputPath + BochsDefaultConfigurationFileName));
      }
      set { SetProperty(BochsEmulatorConfigurationFileString, value); }
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

    public const string EnableBochsDebugString = "EnableBochsDebug";
    public Boolean EnableBochsDebug
    {
        get { return GetProperty(EnableBochsDebugString, false); }
        set { SetProperty(EnableBochsDebugString, value); }
    }
  }
}
