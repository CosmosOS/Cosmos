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
            string name = "ERROR OPCODE NOT DETECTED";

            if (code == OpCodeEnum.Unknown)
            {
                name = "Unknown";
            }
            else if (code == OpCodeEnum.Zero)
            {
                name = "Zero";
            }
            else if (code == OpCodeEnum.One)
            {
                name = "One";
            }
            else if (code == OpCodeEnum.Alias)
            {
                name = "Alias";
            }
            else if (code == OpCodeEnum.Name)
            {
                name = "Name";
            }
            else if (code == OpCodeEnum.Byte)
            {
                name = "Byte";
            }
            else if (code == OpCodeEnum.Word)
            {
                name = "Word";
            }
            else if (code == OpCodeEnum.DWord)
            {
                name = "DWord";
            }
            else if (code == OpCodeEnum.String)
            {
                name = "String";
            }
            else if (code == OpCodeEnum.QWord)
            {
                name = "QWord";
            }
            else if (code == OpCodeEnum.Scope)
            {
                name = "Scope";
            }
            else if (code == OpCodeEnum.Buffer)
            {
                name = "Buffer";
            }
            else if (code == OpCodeEnum.Package)
            {
                name = "Package";
            }
            else if (code == OpCodeEnum.VariablePackage)
            {
                name = "VariablePackage";
            }
            else if (code == OpCodeEnum.Method)
            {
                name = "Method";
            }
            else if (code == OpCodeEnum.External)
            {
                name = "External";
            }
            else if (code == OpCodeEnum.DualNamePrefix)
            {
                name = "DualNamePrefix";
            }
            else if (code == OpCodeEnum.MultiNamePrefix)
            {
                name = "MultiNamePrefix";
            }
            else if (code == OpCodeEnum.ExtendedPrefix)
            {
                name = "ExtendedPrefix";
            }
            else if (code == OpCodeEnum.RootPrefix)
            {
                name = "RootPrefix";
            }
            else if (code == OpCodeEnum.ParentPrefix)
            {
                name = "ParentPrefix";
            }
            else if (code == OpCodeEnum.FirstLocal)
            {
                name = "FirstLocal";
            }
            else if (code == OpCodeEnum.Local0)
            {
                name = "Local0";
            }
            else if (code == OpCodeEnum.Local1)
            {
                name = "Local1";
            }
            else if (code == OpCodeEnum.Local2)
            {
                name = "Local2";
            }
            else if (code == OpCodeEnum.Local3)
            {
                name = "Local3";
            }
            else if (code == OpCodeEnum.Local4)
            {
                name = "Local4";
            }
            else if (code == OpCodeEnum.Local5)
            {
                name = "Local5";
            }
            else if (code == OpCodeEnum.Local6)
            {
                name = "Local6";
            }
            else if (code == OpCodeEnum.Local7)
            {
                name = "Local7";
            }
            else if (code == OpCodeEnum.FirstArg)
            {
                name = "FirstArg";
            }
            else if (code == OpCodeEnum.Arg0)
            {
                name = "Arg0";
            }
            else if (code == OpCodeEnum.Arg1)
            {
                name = "Arg1";
            }
            else if (code == OpCodeEnum.Arg2)
            {
                name = "Arg2";
            }
            else if (code == OpCodeEnum.Arg3)
            {
                name = "Arg3";
            }
            else if (code == OpCodeEnum.Arg4)
            {
                name = "Arg4";
            }
            else if (code == OpCodeEnum.Arg5)
            {
                name = "Arg5";
            }
            else if (code == OpCodeEnum.Arg6)
            {
                name = "Arg6";
            }
            else if (code == OpCodeEnum.Store)
            {
                name = "Store";
            }
            else if (code == OpCodeEnum.ReferenceOf)
            {
                name = "ReferenceOf";
            }
            else if (code == OpCodeEnum.Add)
            {
                name = "Add";
            }
            else if (code == OpCodeEnum.Concatenate)
            {
                name = "Concatenate";
            }
            else if (code == OpCodeEnum.Subtract)
            {
                name = "Subtract";
            }
            else if (code == OpCodeEnum.Increment)
            {
                name = "Increment";
            }
            else if (code == OpCodeEnum.Decrement)
            {
                name = "Decrement";
            }
            else if (code == OpCodeEnum.Multiply)
            {
                name = "Multiply";
            }
            else if (code == OpCodeEnum.Divide)
            {
                name = "Divide";
            }
            else if (code == OpCodeEnum.ShiftLeft)
            {
                name = "ShiftLeft";
            }
            else if (code == OpCodeEnum.ShiftRight)
            {
                name = "ShiftRight";
            }
            else if (code == OpCodeEnum.BitAnd)
            {
                name = "BitAnd";
            }
            else if (code == OpCodeEnum.BitNand)
            {
                name = "BitNand";
            }
            else if (code == OpCodeEnum.BitOr)
            {
                name = "BitOr";
            }
            else if (code == OpCodeEnum.BitNor)
            {
                name = "BitNor";
            }
            else if (code == OpCodeEnum.BitXor)
            {
                name = "BitXor";
            }
            else if (code == OpCodeEnum.BitNot)
            {
                name = "BitNot";
            }
            else if (code == OpCodeEnum.FindSetLeftBit)
            {
                name = "FindSetLeftBit";
            }
            else if (code == OpCodeEnum.FindSetRightBit)
            {
                name = "FindSetRightBit";
            }
            else if (code == OpCodeEnum.DereferenceOf)
            {
                name = "DereferenceOf";
            }
            else if (code == OpCodeEnum.ConcatenateTemplate)
            {
                name = "ConcatenateTemplate";
            }
            else if (code == OpCodeEnum.Mod)
            {
                name = "Mod";
            }
            else if (code == OpCodeEnum.Notify)
            {
                name = "Notify";
            }
            else if (code == OpCodeEnum.SizeOf)
            {
                name = "SizeOf";
            }
            else if (code == OpCodeEnum.Index)
            {
                name = "Index";
            }
            else if (code == OpCodeEnum.Match)
            {
                name = "Match";
            }
            else if (code == OpCodeEnum.CreateDWordField)
            {
                name = "CreateDWordField";
            }
            else if (code == OpCodeEnum.CreateWordField)
            {
                name = "CreateWordField";
            }
            else if (code == OpCodeEnum.CreateByteField)
            {
                name = "CreateByteField";
            }
            else if (code == OpCodeEnum.CreateBitField)
            {
                name = "CreateBitField";
            }
            else if (code == OpCodeEnum.ObjectType)
            {
                name = "ObjectType";
            }
            else if (code == OpCodeEnum.CreateQWordField)
            {
                name = "CreateQWordField";
            }
            else if (code == OpCodeEnum.LogicalAnd)
            {
                name = "LogicalAnd";
            }
            else if (code == OpCodeEnum.LogicalOr)
            {
                name = "LogicalOr";
            }
            else if (code == OpCodeEnum.LogicalNot)
            {
                name = "LogicalNot";
            }
            else if (code == OpCodeEnum.LogicalEqual)
            {
                name = "LogicalEqual";
            }
            else if (code == OpCodeEnum.LogicalGreater)
            {
                name = "LogicalGreater";
            }
            else if (code == OpCodeEnum.LogicalLess)
            {
                name = "LogicalLess";
            }
            else if (code == OpCodeEnum.ToBuffer)
            {
                name = "ToBuffer";
            }
            else if (code == OpCodeEnum.ToDecimalString)
            {
                name = "ToDecimalString";
            }
            else if (code == OpCodeEnum.ToHexString)
            {
                name = "ToHexString";
            }
            else if (code == OpCodeEnum.ToInteger)
            {
                name = "ToInteger";
            }
            else if (code == OpCodeEnum.ToString)
            {
                name = "ToString";
            }
            else if (code == OpCodeEnum.CopyObject)
            {
                name = "CopyObject";
            }
            else if (code == OpCodeEnum.Mid)
            {
                name = "Mid";
            }
            else if (code == OpCodeEnum.Continue)
            {
                name = "Continue";
            }
            else if (code == OpCodeEnum.If)
            {
                name = "If";
            }
            else if (code == OpCodeEnum.Else)
            {
                name = "Else";
            }
            else if (code == OpCodeEnum.While)
            {
                name = "While";
            }
            else if (code == OpCodeEnum.NoOp)
            {
                name = "NoOp";
            }
            else if (code == OpCodeEnum.Return)
            {
                name = "Return";
            }
            else if (code == OpCodeEnum.Break)
            {
                name = "Break";
            }
            else if (code == OpCodeEnum.Comment)
            {
                name = "Comment";
            }
            else if (code == OpCodeEnum.Breakpoint)
            {
                name = "Breakpoint";
            }
            else if (code == OpCodeEnum.Ones)
            {
                name = "Ones";
            }
            else if (code == OpCodeEnum.LogicalGreaterEqual)
            {
                name = "LogicalGreaterEqual";
            }
            else if (code == OpCodeEnum.LogicalLessEqual)
            {
                name = "LogicalLessEqual";
            }
            else if (code == OpCodeEnum.LogicalNotEqual)
            {
                name = "LogicalNotEqual";
            }
            else if (code == OpCodeEnum.ExtendedOpcode)
            {
                name = "ExtendedOpcode";
            }
            else if (code == OpCodeEnum.Mutex)
            {
                name = "Mutex";
            }
            else if (code == OpCodeEnum.Event)
            {
                name = "Event";
            }
            else if (code == OpCodeEnum.ShiftRightBit)
            {
                name = "ShiftRightBit";
            }
            else if (code == OpCodeEnum.ShiftLeftBit)
            {
                name = "ShiftLeftBit";
            }
            else if (code == OpCodeEnum.ConditionalReferenceOf)
            {
                name = "ConditionalReferenceOf";
            }
            else if (code == OpCodeEnum.CreateField)
            {
                name = "CreateField";
            }
            else if (code == OpCodeEnum.LoadTable)
            {
                name = "LoadTable";
            }
            else if (code == OpCodeEnum.Load)
            {
                name = "Load";
            }
            else if (code == OpCodeEnum.Stall)
            {
                name = "Stall";
            }
            else if (code == OpCodeEnum.Sleep)
            {
                name = "Sleep";
            }
            else if (code == OpCodeEnum.Acquire)
            {
                name = "Acquire";
            }
            else if (code == OpCodeEnum.Signal)
            {
                name = "Signal";
            }
            else if (code == OpCodeEnum.Wait)
            {
                name = "Wait";
            }
            else if (code == OpCodeEnum.Reset)
            {
                name = "Reset";
            }
            else if (code == OpCodeEnum.Release)
            {
                name = "Release";
            }
            else if (code == OpCodeEnum.FromBCD)
            {
                name = "FromBCD";
            }
            else if (code == OpCodeEnum.ToBCD)
            {
                name = "ToBCD";
            }
            else if (code == OpCodeEnum.Unload)
            {
                name = "Unload";
            }
            else if (code == OpCodeEnum.Revision)
            {
                name = "Revision";
            }
            else if (code == OpCodeEnum.Debug)
            {
                name = "Debug";
            }
            else if (code == OpCodeEnum.Fatal)
            {
                name = "Fatal";
            }
            else if (code == OpCodeEnum.Timer)
            {
                name = "Timer";
            }
            else if (code == OpCodeEnum.Region)
            {
                name = "Region";
            }
            else if (code == OpCodeEnum.Field)
            {
                name = "Field";
            }
            else if (code == OpCodeEnum.Device)
            {
                name = "Device";
            }
            else if (code == OpCodeEnum.Processor)
            {
                name = "Processor";
            }
            else if (code == OpCodeEnum.PowerResource)
            {
                name = "PowerResource";
            }
            else if (code == OpCodeEnum.ThermalZone)
            {
                name = "ThermalZone";
            }
            else if (code == OpCodeEnum.IndexField)
            {
                name = "IndexField";
            }
            else if (code == OpCodeEnum.BankField)
            {
                name = "BankField";
            }
            else if (code == OpCodeEnum.DataRegion)
            {
                name = "DataRegion";
            }
            else if (code == OpCodeEnum.FieldOffset)
            {
                name = "FieldOffset";
            }
            else if (code == OpCodeEnum.FieldAccess)
            {
                name = "FieldAccess";
            }
            else if (code == OpCodeEnum.FieldConnection)
            {
                name = "FieldConnection";
            }
            else if (code == OpCodeEnum.FieldExternalAccess)
            {
                name = "FieldExternalAccess";
            }
            else if (code == OpCodeEnum.NamePath)
            {
                name = "NamePath";
            }
            else if (code == OpCodeEnum.NamedField)
            {
                name = "NamedField";
            }
            else if (code == OpCodeEnum.ReservedField)
            {
                name = "ReservedField";
            }
            else if (code == OpCodeEnum.AccessField)
            {
                name = "AccessField";
            }
            else if (code == OpCodeEnum.ByteList)
            {
                name = "ByteList";
            }
            else if (code == OpCodeEnum.MethodCall)
            {
                name = "MethodCall";
            }
            else if (code == OpCodeEnum.ReturnValue)
            {
                name = "ReturnValue";
            }
            else if (code == OpCodeEnum.EvalSubtree)
            {
                name = "EvalSubtree";
            }
            else if (code == OpCodeEnum.Connection)
            {
                name = "Connection";
            }
            else if (code == OpCodeEnum.ExternalAccessField)
            {
                name = "ExternalAccessField";
            }

            return name;
        }

        public override string ToString()
		{
			return Name;
		}
	}
}
