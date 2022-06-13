//Based on the ACPICA source code (https://github.com/acpica/acpica)
using System;

namespace ACPILib.AML
{
	public class OpCode
	{
		public string Name
		{
			get;
			private set;
		}

		public ushort CodeValue
		{
			get { return (ushort)Code; }
		}

		public int CodeByteSize
		{
			get
			{
				if (CodeValue > 0x00FF)
					return 2;

				return 1;
			}
		}

		public OpCodeEnum Code;
		public ParseArgFlags[] ParseArgs;
		public InterpreterArgFlags[] RuntimeArgs;
		public ObjectTypeEnum ObjectType;
		public OpCodeClass Class;
		public OpCodeType Type;
		public OpCodeFlags Flags;

		public OpCode(string name, ParseArgFlags[] parseArgs, InterpreterArgFlags[] rtArgs, ObjectTypeEnum objectType, OpCodeClass cls, OpCodeType type, OpCodeFlags flags)
		{
			Code = OpCodeEnum.Unknown;
			Name = name;
			ParseArgs = parseArgs;
			RuntimeArgs = rtArgs;
			ObjectType = objectType;
			Class = cls;
			Type = type;
		}

		public OpCode(OpCodeEnum code, ParseArgFlags[] parseArgs, InterpreterArgFlags[] rtArgs, ObjectTypeEnum objectType, OpCodeClass cls, OpCodeType type, OpCodeFlags flags)
		{
			Code = code;
			ParseArgs = parseArgs;
			RuntimeArgs = rtArgs;
			ObjectType = objectType;
			Class = cls;
			Type = type;
			Flags = flags;

			Name = GetName(code);
		}

        private string GetName(OpCodeEnum code)
        {
            return "Unknown Opcode";
        }

		public override string ToString()
		{
			return Name;
		}
	}
}
