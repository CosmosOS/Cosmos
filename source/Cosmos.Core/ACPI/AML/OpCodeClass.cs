using System;

namespace ACPILib.AML
{
	[Flags]
	public enum OpCodeClass : byte
	{
		Execute = 0x00,
		Create = 0x01,
		Argument = 0x02,
		NamedObject = 0x03,
		Control = 0x04,
		ASCII = 0x05,
		Prefix = 0x06,
		Internal = 0x07,
		ReturnValue = 0x08,
		MethodCall = 0x09,
		ClassUnknown = 0x0A,
	}
}
