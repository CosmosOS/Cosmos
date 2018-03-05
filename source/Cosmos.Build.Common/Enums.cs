using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Reflection;

namespace Cosmos.Build.Common
{

    public enum DeploymentType
    {
        [Description("ISO Image")]
        ISO,
        [Description("USB Device")]
        USB,
        [Description("PXE Network Boot")]
        PXE,
        BinaryImage
    }

    public enum LaunchType
    {
        [Description("None")]
        None,
        [Description("VMware")]
        VMware,
        [Description("Attached Slave (CanaKit)")]
        Slave,
        [Description("Bochs")]
        Bochs,
        [Description("Intel Edison")]
        IntelEdison,
        [Description("Hyper-V")]
        HyperV
    }

    public enum VMwareEdition
    {
        Workstation,
        Player
    }

    public enum Architecture
    {
        x86 //, x64
    }

    public enum Framework
    {
        [Description("Microsoft .NET")]
        MicrosoftNET,
        Mono
    }

    public enum LogSeverityEnum : byte
    {
        Warning = 0,
        Error = 1,
        Informational = 2,
        Performance = 3
    }

    public enum TraceAssemblies
    {
        None = 0,
        User = 1,
        Cosmos = 2,
        All = 3
    };

    public enum DebugMode
    {
        IL,
        Source
    }

    public enum StackCorruptionDetectionLevel
    {
        [Description("All Instructions")]
        AllInstructions,
        [Description("Method Footers Only")]
        MethodFooters
    }

    public static class EnumHelper
    {
        public static string GetDescription(this Enum value)
        {
            FieldInfo field = value.GetType().GetRuntimeField(value.ToString());
            DescriptionAttribute attribute = field.GetCustomAttribute<DescriptionAttribute>();
            return attribute == null ? value.ToString() : attribute.Description;
        }
    }
}
