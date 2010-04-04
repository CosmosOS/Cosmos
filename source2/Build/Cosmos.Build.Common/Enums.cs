using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace Cosmos.Build.Common {

	public enum TargetHost
	{
	  [Description("VMWare Workstation")]
	  VMWareWorkstation,
	  [Description("VMWare Server")]
	  VMWareServer,
	  QEMU,
	  [Description("Virtual PC")]
	  VPC,
	  PXE,
	  ISO
	}

	public enum Architecture
	{
	  x86
	  //x64
	}

	public enum Framework
	{
		[Description("Microsoft .NET")]
		MicrosoftNET,
		Mono
	}

	public enum VMQemuNetworkCard
	{
		None,
		[Description("Realtek RTL8139")]
		RealtekRTL8139,
	}

	public enum VMQemuAudioCard
	{
		None,
		[Description("PC Speaker")]
		PCSpeaker
	}

	public enum DebugQemuCommunication
	{
		None,
		[Description("TCP: Cosmos client, QEMU server")] 
		TCPListener,
		[Description("TCP: QEMU client, Cosmos server")] 
		TCPClient,
        [Description("Pipe: Cosmos client, QEMU server")]
		NamedPipeListener,
		[Description("Pipe: QEMU client, Cosmos server")]
		NamedPipeClient
	}

    public enum LogSeverityEnum : byte
    {
        Warning = 0, Error = 1, Informational = 2, Performance = 3
    }
    public enum TraceAssemblies { All, Cosmos, User };
    public enum DebugMode { None, IL, Source, GDB }

	public class DescriptionAttribute : Attribute
	{
		public static String GetDescription(object value)
		{
			Type valueType = value.GetType();
			MemberInfo[] valueMemberInfo;
			Object[] valueMemberAttribute;

			if (valueType.IsEnum == true)
				{
					valueMemberInfo = valueType.GetMember(value.ToString());

					if ((valueMemberInfo != null) && (valueMemberInfo.Length > 0))
					{
						valueMemberAttribute = valueMemberInfo[0].GetCustomAttributes(typeof(DescriptionAttribute),false);
						if ((valueMemberAttribute != null) && (valueMemberAttribute.Length > 0))
						{
							return ((DescriptionAttribute)valueMemberAttribute[0]).Description;
						}
					}
				}

				valueMemberAttribute = valueType.GetCustomAttributes(typeof(DescriptionAttribute),false);
				if ((valueMemberAttribute != null) && (valueMemberAttribute.Length > 0))
				{
					return ((DescriptionAttribute)valueMemberAttribute[0]).Description;
				}

				return value.ToString();
		}

		private string emDescription;

		public DescriptionAttribute(String description)
		{
			this.emDescription = description;
		}

		public String Description
		{ get { return this.emDescription; } }
	}
}
