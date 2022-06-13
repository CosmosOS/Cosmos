using System;

namespace ACPILib.AML
{
	public enum OpCodeType : byte
	{
		Execute_0A_0T_1R = 0x00,
		Execute_1A_0T_0R = 0x01,
		Execute_1A_0T_1R = 0x02,
		Execute_1A_1T_0R = 0x03,
		Execute_1A_1T_1R = 0x04,
		Execute_2A_0T_0R = 0x05,
		Execute_2A_0T_1R = 0x06,
		Execute_2A_1T_1R = 0x07,
		Execute_2A_2T_1R = 0x08,
		Execute_3A_0T_0R = 0x09,
		Execute_3A_1T_1R = 0x0A,
		Execute_6A_0T_1R = 0x0B,

		Literal = 0x0C,
		Constant = 0x0D,
		MethodArgument = 0x0E,
		LocalVariable = 0x0F,
		DataTerm = 0x10,

		MethodCall = 0x11,

		CreateField = 0x12,
		CreateObject = 0x13,
		Control = 0x14,
		NamedNoObject = 0x15,
		NamedField = 0x16,
		NamedSimple = 0x17,
		NamedComplex = 0x18,
		Return = 0x19,
		Undefined = 0x1A,
		Bogus = 0x1B,
	}
}
