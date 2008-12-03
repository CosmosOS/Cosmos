using System;
using System.Collections.Generic;
using System.IO;


using CPU = Indy.IL2CPU.Assembler;
using CPUx86 = Indy.IL2CPU.Assembler.X86;
using System.Reflection;
using Indy.IL2CPU.Assembler;

namespace Indy.IL2CPU.IL.X86 {
	[OpCode(OpCodeEnum.Castclass)]
	public class Castclass: Op {
		private int mTypeId;
		private string mThisLabel;
		private string mNextOpLabel;
		private Type mCastAsType;
		private int mCurrentILOffset;
		private MethodInformation mMethodInfo;

        public static void ScanOp(ILReader aReader, MethodInformation aMethodInfo, SortedList<string, object> aMethodData) {
            Type xType = aReader.OperandValueType;
            if (xType == null)
            {
                throw new Exception("Unable to determine Type!");
            }
            Engine.RegisterType(xType);
            Call.ScanOp(Engine.GetMethodBase(typeof(VTablesImpl),
                                             "IsInstance",
                                             "System.Int32",
                                             "System.Int32"));
            Newobj.ScanOp(typeof(InvalidCastException).GetConstructor(new Type[0]));
        }

		public Castclass(ILReader aReader, MethodInformation aMethodInfo)
			: base(aReader, aMethodInfo) {
			Type xType = aReader.OperandValueType;
			if (xType == null) {
				throw new Exception("Unable to determine Type!");
			}
			mCastAsType = xType;
			mTypeId = Engine.RegisterType(mCastAsType);
			mThisLabel = GetInstructionLabel(aReader);
			mNextOpLabel = GetInstructionLabel(aReader.NextPosition);
			mCurrentILOffset = (int)aReader.Position;
			mMethodInfo = aMethodInfo;
		}

		public override void DoAssemble() {
			// todo: throw an exception when the class does not support the cast!
			string mReturnNullLabel = mThisLabel + "_ReturnNull";
            new CPUx86.Move { DestinationReg = CPUx86.Registers.EAX, SourceReg = CPUx86.Registers.ESP, SourceIsIndirect = true };
            new CPUx86.Compare { DestinationReg = CPUx86.Registers.EAX, SourceValue = 0 };
            new CPUx86.ConditionalJump { Condition = CPUx86.ConditionalTestEnum.Zero, DestinationLabel = mReturnNullLabel };
            new CPUx86.Push { DestinationReg = CPUx86.Registers.EAX, DestinationIsIndirect = true };
			new CPUx86.Push{DestinationValue=(uint)mTypeId};
			Assembler.StackContents.Push(new StackContent(4, typeof(object)));
			Assembler.StackContents.Push(new StackContent(4, typeof(object)));
			MethodBase xMethodIsInstance = Engine.GetMethodBase(typeof(VTablesImpl), "IsInstance", "System.Int32", "System.Int32");
			Engine.QueueMethod(xMethodIsInstance);
            Op xOp = new Call(xMethodIsInstance, (uint)mCurrentILOffset, mMethodInfo.DebugMode, mThisLabel + "_After_IsInstance_Call");
			xOp.Assembler = Assembler;
			xOp.Assemble();
		    new CPU.Label(mThisLabel + "_After_IsInstance_Call");
			Assembler.StackContents.Pop();
            new CPUx86.Pop { DestinationReg = CPUx86.Registers.EAX };
            new CPUx86.Compare { DestinationReg = CPUx86.Registers.EAX, SourceValue = 0 };
            new CPUx86.ConditionalJump { Condition = CPUx86.ConditionalTestEnum.NotEqual, DestinationLabel = mNextOpLabel };
			new CPU.Label(mReturnNullLabel);
            new CPUx86.Add { DestinationReg = CPUx86.Registers.ESP, SourceValue = 4 };
            Newobj.Assemble(Assembler, typeof(InvalidCastException).GetConstructor(new Type[0]), Engine.RegisterType(typeof(InvalidCastException)), mThisLabel, mMethodInfo, mCurrentILOffset, mThisLabel + "_After_NewException");
            new CPU.Label(mThisLabel + "_After_NewException");
			Call.EmitExceptionLogic(Assembler, (uint)mCurrentILOffset, mMethodInfo, mNextOpLabel, false, null);
		}
	}
}