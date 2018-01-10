using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Cosmos.Build.Common
{
    public class BuildProperties : PropertiesBase
    {
        public static List<string> PropNames = new List<string>();

        static BuildProperties()
        {
            var xFields = typeof(BuildPropertyNames).GetRuntimeFields();

            foreach (var xField in xFields)
            {
                // IsLiteral determines if its value is written at compile time and not changeable.
                // Consts are static even if we dont use static keyword.
                // IsInitOnly determine if the field can be set in the body of the constructor
                // for C# a field which is readonly keyword would have both true but a const field would have only IsLiteral equal to true
                if (xField.IsLiteral && !xField.IsInitOnly && xField.FieldType == typeof(string))
                {
                    string xName = (string)xField.GetValue(null);
                    if (xName != BuildPropertyNames.ProfileString)
                    {
                        PropNames.Add(xName);
                    }
                }
            }
        }

        /// <summary>
        /// Gets array of project names which are project independent.
        /// </summary>
        public override string[] ProjectIndependentProperties
        {
            get
            {
                return new string[] { BuildPropertyNames.BinFormatString };
            }
        }

        /// <summary>
        /// Save properties under selected profile.
        /// </summary>
        /// <param name="aName">Name of the profile for which save properties.</param>
        public void SaveProfile(string aName)
        {
            foreach (var xName in BuildProperties.PropNames)
            {
                // Skip project independent properties.
                if (this.ProjectIndependentProperties.Contains(xName))
                {
                    continue;
                }

                string xValue = GetProperty(xName);
                if (!string.IsNullOrWhiteSpace(xValue))
                {
                    SetProperty(aName + "_" + xName, xValue);
                }
            }
        }

        /// <summary>
        /// Load properties for the given profile.
        /// </summary>
        /// <param name="aName">Name of the profile for which load properties.</param>
        public void LoadProfile(string aName)
        {
            foreach (var xName in BuildProperties.PropNames)
            {
                string xValue;
                // Skip project independent properties.
                if (this.ProjectIndependentProperties.Contains(xName))
                {
                    xValue = GetProperty(xName);
                }
                else
                {
                    xValue = GetProperty(aName + "_" + xName);
                }

                if (!string.IsNullOrWhiteSpace(xValue))
                {
                    SetProperty(xName, xValue);
                }
            }

            // Reforce fixed settings for presets on each load.
            if (aName == "ISO")
            {
                Description = "Creates a bootable ISO image which can be burned to a DVD."
                              + " After running the selected project, an explorer window will open containing the ISO file."
                              + " The ISO file can then be burned to a CD or DVD and used to boot a physical or virtual system.";
                Deployment = DeploymentType.ISO;
                Launch = LaunchType.None;

            }
            else if (aName == "USB")
            {
                Description = "Makes a USB device such as a flash drive or external hard disk bootable.";
                Deployment = DeploymentType.USB;
                Launch = LaunchType.None;
            }
            else if (aName == "VMware")
            {
                Description = "Use VMware Player or Workstation to deploy and debug.";
                Deployment = DeploymentType.ISO;
                Launch = LaunchType.VMware;
                VisualStudioDebugPort = @"Pipe: Cosmos\Serial";
            }
            else if (aName == "PXE")
            {
                Description =
                    "Creates a PXE setup and hosts a DCHP and TFTP server to deploy directly to physical hardware. Allows debugging with a serial cable.";
                Deployment = DeploymentType.PXE;
                Launch = LaunchType.None;

            }
            else if (aName == "Bochs")
            {
                Description = "Use Bochs emulator to deploy and debug.";
                Deployment = DeploymentType.ISO;
                Launch = LaunchType.Bochs;
                VisualStudioDebugPort = @"Pipe: Cosmos\Serial";
            }
            else if (aName == "IntelEdison")
            {
                Description = "Connect to Intel Edison device to deploy and debug.";
                Deployment = DeploymentType.BinaryImage;
                Launch = LaunchType.IntelEdison;
            }
            else if (aName == "HyperV")
            {
                Description = "Use Hyper-V to deploy and debug.";
                Deployment = DeploymentType.ISO;
                Launch = LaunchType.HyperV;
                VisualStudioDebugPort = "Pipe: CosmosSerial";
            }
        }

        public void DeleteProfile(string aPrefix)
        {
            foreach (var xName in BuildProperties.PropNames)
            {
                mPropTable.Remove(aPrefix + "_" + xName);
            }
        }

        // Profile
        public string Profile
        {
            get
            {
                return GetProperty(BuildPropertyNames.ProfileString, "VMware");
            }
            set
            {
                SetProperty(BuildPropertyNames.ProfileString, value);
            }
        }

        public string Name
        {
            get
            {
                return GetProperty(BuildPropertyNames.NameString, "");
            }
            set
            {
                SetProperty(BuildPropertyNames.NameString, value);
            }
        }

        public string Description
        {
            get
            {
                return GetProperty(BuildPropertyNames.DescriptionString, "");
            }
            set
            {
                SetProperty(BuildPropertyNames.DescriptionString, value);
            }
        }

        // Deployment
        public DeploymentType Deployment
        {
            get
            {
                return GetProperty(BuildPropertyNames.DeploymentString, DeploymentType.ISO);
            }
            set
            {
                SetProperty(BuildPropertyNames.DeploymentString, value);
            }
        }

        // Launch
        public LaunchType Launch
        {
            get
            {
                return GetProperty(BuildPropertyNames.LaunchString, LaunchType.VMware);
            }
            set
            {
                SetProperty(BuildPropertyNames.LaunchString, value);
            }
        }

        public bool ShowLaunchConsole
        {
            get
            {
                return GetProperty(BuildPropertyNames.ShowLaunchConsoleString, false);
            }
            set
            {
                SetProperty(BuildPropertyNames.ShowLaunchConsoleString, value);
            }
        }

        // Debug
        public bool DebugEnabled
        {
            get
            {
                return GetProperty(BuildPropertyNames.DebugEnabledString, true);
            }
            set
            {
                SetProperty(BuildPropertyNames.DebugEnabledString, value);
            }
        }

        public bool StackCorruptionDetectionEnabled
        {
            get
            {
                return GetProperty(BuildPropertyNames.StackCorruptionDetectionEnabledString, true);
            }
            set
            {
                SetProperty(BuildPropertyNames.StackCorruptionDetectionEnabledString, value);
            }
        }

        public StackCorruptionDetectionLevel StackCorruptionDetectionLevel
        {
            get
            {
                return GetProperty(BuildPropertyNames.StackCorruptionDetectionLevelString, StackCorruptionDetectionLevel.MethodFooters);
            }
            set
            {
                SetProperty(BuildPropertyNames.StackCorruptionDetectionLevelString, value);
            }
        }

        public DebugMode DebugMode
        {
            get
            {
                return GetProperty(BuildPropertyNames.DebugModeString, DebugMode.Source);
            }
            set
            {
                SetProperty(BuildPropertyNames.DebugModeString, value);
            }
        }

        public bool IgnoreDebugStubAttribute
        {
            get
            {
                return GetProperty(BuildPropertyNames.IgnoreDebugStubAttributeString, false);
            }
            set
            {
                SetProperty(BuildPropertyNames.IgnoreDebugStubAttributeString, value);
            }
        }

        public string CosmosDebugPort
        {
            get
            {
                return GetProperty(BuildPropertyNames.CosmosDebugPortString, "Serial: COM1");
            }
            set
            {
                SetProperty(BuildPropertyNames.CosmosDebugPortString, value);
            }
        }

        public string VisualStudioDebugPort
        {
            get
            {
                return GetProperty(BuildPropertyNames.VisualStudioDebugPortString, "Serial: COM1");
            }
            set
            {
                SetProperty(BuildPropertyNames.VisualStudioDebugPortString, value);
            }
        }

        // PXE
        public string PxeInterface
        {
            get
            {
                return GetProperty(BuildPropertyNames.PxeInterfaceString, "");
            }
            set
            {
                SetProperty(BuildPropertyNames.PxeInterfaceString, value);
            }
        }

        public string SlavePort
        {
            get
            {
                return GetProperty(BuildPropertyNames.SlavePortString, "None");
            }
            set
            {
                SetProperty(BuildPropertyNames.SlavePortString, value);
            }
        }

        // Bochs
        public const string BochsDefaultConfigurationFileName = "Cosmos.bxrc";

        public const string BochsEmulatorConfigurationFileString = "BochsConfig";

        public string BochsEmulatorConfigurationFile
        {
            get
            {
                return GetProperty(
                    BochsEmulatorConfigurationFileString,
                    Path.Combine(OutputPath + BochsDefaultConfigurationFileName));
            }
            set
            {
                SetProperty(BochsEmulatorConfigurationFileString, value);
            }
        }

        // VMware
        public VMwareEdition VMwareEdition
        {
            get
            {
                return GetProperty(BuildPropertyNames.VMwareEditionString, VMwareEdition.Player);
            }
            set
            {
                SetProperty(BuildPropertyNames.VMwareEditionString, value);
            }
        }

        public String OutputPath
        {
            get
            {
                return GetProperty(BuildPropertyNames.OutputPathString, @"bin\debug");
            }
            set
            {
                SetProperty(BuildPropertyNames.OutputPathString, value);
            }
        }

        public Framework Framework
        {
            get
            {
                return GetProperty(BuildPropertyNames.FrameworkString, Common.Framework.MicrosoftNET);
            }
            set
            {
                SetProperty(BuildPropertyNames.FrameworkString, value);
            }
        }

        public Boolean UseInternalAssembler
        {
            get
            {
                return GetProperty(BuildPropertyNames.UseInternalAssemblerString, false);
            }
            set
            {
                SetProperty(BuildPropertyNames.UseInternalAssemblerString, value);
            }
        }

        public TraceAssemblies TraceAssemblies
        {
            get
            {
                return GetProperty(BuildPropertyNames.TraceAssembliesString, TraceAssemblies.User);
            }
            set
            {
                SetProperty(BuildPropertyNames.TraceAssembliesString, value);
            }
        }

        public bool EnableGDB
        {
            get
            {
                return GetProperty(BuildPropertyNames.EnableGDBString, false);
            }
            set
            {
                SetProperty(BuildPropertyNames.EnableGDBString, value);
            }
        }

        public bool StartCosmosGDB
        {
            get
            {
                return GetProperty(BuildPropertyNames.StartCosmosGDBString, false);
            }
            set
            {
                SetProperty(BuildPropertyNames.StartCosmosGDBString, value);
            }
        }

        public bool EnableBochsDebug
        {
            get
            {
                return GetProperty(BuildPropertyNames.EnableBochsDebugString, false);
            }
            set
            {
                SetProperty(BuildPropertyNames.EnableBochsDebugString, value);
            }
        }

        public bool StartBochsDebugGui
        {
            get
            {
                return GetProperty(BuildPropertyNames.StartBochsDebugGui, false);
            }
            set
            {
                SetProperty(BuildPropertyNames.StartBochsDebugGui, value);
            }
        }

        /// <summary>
        /// Gets or sets binary format which is used for producing kernel image.
        /// </summary>
        public BinFormat BinFormat
        {
            get
            {
                return GetProperty(BuildPropertyNames.BinFormatString, BinFormat.Bin);
            }
            set
            {
                SetProperty(BuildPropertyNames.BinFormatString, value);
            }
        }
    }
}
