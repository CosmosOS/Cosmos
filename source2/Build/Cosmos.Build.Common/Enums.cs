using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace Cosmos.Build.Common {

  public enum ProfileType {
    Custom,
    [Description("ISO Image")]
    ISO,
    [Description("USB Device")]
    USB,
    [Description("VMware")]
    VMware,
    [Description("PXE Network Boot")]
    PXE
  }

  public enum Deployment {
    [Description("ISO Image")]
    ISO,
    [Description("USB Device")]
    USB,
    [Description("PXE Network Boot")]
    PXE
  }

  public enum Launch {
    [Description("None or Manual")]
    None,
    [Description("VMware")]
    VMware,
    [Description("PXE Server")]
    PXE,
    [Description("Specially attached slave")]
    Slave
  }

  public enum VMwareEdition {
    Workstation,
    Player
  }

  public enum Architecture {
    x86 //, x64 
  }

  public enum Framework {
    [Description("Microsoft .NET")]
    MicrosoftNET,
    Mono
  }

  public enum LogSeverityEnum : byte {
    Warning = 0, Error = 1, Informational = 2, Performance = 3
  }
  public enum TraceAssemblies { All, Cosmos, User };
  public enum DebugMode { IL, Source }

  public sealed class DescriptionAttribute : Attribute {
    public static String GetDescription(object value) {
      Type valueType = value.GetType();
      MemberInfo[] valueMemberInfo;
      Object[] valueMemberAttribute;

      if (valueType.IsEnum) {
        valueMemberInfo = valueType.GetMember(value.ToString());

        if ((valueMemberInfo != null) && (valueMemberInfo.Length > 0)) {
          valueMemberAttribute = valueMemberInfo[0].GetCustomAttributes(typeof(DescriptionAttribute), false);
          if ((valueMemberAttribute != null) && (valueMemberAttribute.Length > 0)) {
            return ((DescriptionAttribute)valueMemberAttribute[0]).Description;
          }
        }
      }

      valueMemberAttribute = valueType.GetCustomAttributes(typeof(DescriptionAttribute), false);
      if ((valueMemberAttribute != null) && (valueMemberAttribute.Length > 0)) {
        return ((DescriptionAttribute)valueMemberAttribute[0]).Description;
      }

      return value.ToString();
    }

    private string emDescription;
    public DescriptionAttribute(String description) {
      emDescription = description;
    }

    public String Description { get { return emDescription; } }
  }
}
