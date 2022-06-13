using System;

namespace ACPILib.AML
{
	[Flags]
	public enum OpCodeFlags : ushort
	{
		Logical = 0x0001,
		LogicalNumeric = 0x0002,
		Math = 0x0004,
		Create = 0x0008,
		Field = 0x0010,
		Defer = 0x0020,
		Named = 0x0040,
		NSNode = 0x0080,
		NSOpCode = 0x0100,
		NSObject = 0x0200,
		HasReturnValue = 0x0400,
		HasTarget = 0x0800,
		HasArguments = 0x1000,
		Constant = 0x2000,
		NoOperandResolve = 0x4000,
	}

	class OpCodeExtendedFlags
	{
		public static OpCodeFlags AML_FLAGS_EXEC_0A_0T_1R = OpCodeFlags.HasReturnValue;
		public static OpCodeFlags AML_FLAGS_EXEC_1A_0T_0R = OpCodeFlags.HasArguments;
		public static OpCodeFlags AML_FLAGS_EXEC_1A_0T_1R = OpCodeFlags.HasArguments | OpCodeFlags.HasReturnValue;
		public static OpCodeFlags AML_FLAGS_EXEC_1A_1T_0R = OpCodeFlags.HasArguments | OpCodeFlags.HasTarget;
		public static OpCodeFlags AML_FLAGS_EXEC_1A_1T_1R = OpCodeFlags.HasArguments | OpCodeFlags.HasTarget | OpCodeFlags.HasReturnValue;
		public static OpCodeFlags AML_FLAGS_EXEC_2A_0T_0R = OpCodeFlags.HasArguments;
		public static OpCodeFlags AML_FLAGS_EXEC_2A_0T_1R = OpCodeFlags.HasArguments | OpCodeFlags.HasReturnValue;
		public static OpCodeFlags AML_FLAGS_EXEC_2A_1T_1R = OpCodeFlags.HasArguments | OpCodeFlags.HasTarget | OpCodeFlags.HasReturnValue;
		public static OpCodeFlags AML_FLAGS_EXEC_2A_2T_1R = OpCodeFlags.HasArguments | OpCodeFlags.HasTarget | OpCodeFlags.HasReturnValue;
		public static OpCodeFlags AML_FLAGS_EXEC_3A_0T_0R = OpCodeFlags.HasArguments;
		public static OpCodeFlags AML_FLAGS_EXEC_3A_1T_1R = OpCodeFlags.HasArguments | OpCodeFlags.HasTarget | OpCodeFlags.HasReturnValue;
		public static OpCodeFlags AML_FLAGS_EXEC_6A_0T_1R = OpCodeFlags.HasArguments | OpCodeFlags.HasReturnValue;
	}
}
