using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.ComponentModel;

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

    public sealed class DescriptionAttribute : Attribute
    {
        public static String GetDescription(object value)
        {
            Type valueType = value.GetType();
            MemberInfo[] valueMemberInfo;
            DescriptionAttribute valueMemberAttribute;

            if (valueType.GetTypeInfo().IsEnum)
            {
                var xTypeInfo = valueType.GetTypeInfo();
                var xMemberInfo = xTypeInfo.GetMember(value.ToString());
                if (xMemberInfo.Any())
                {
                    valueMemberInfo = valueType.GetTypeInfo().GetMember(value.ToString());
                    valueMemberAttribute = valueMemberInfo[0].GetCustomAttribute<DescriptionAttribute>();
                    return valueMemberAttribute.Description;
                }
            }
            return null;
        }

        private string emDescription;

        public DescriptionAttribute(String description)
        {
            emDescription = description;
        }

        public String Description
        {
            get { return emDescription; }
        }
    }

}
