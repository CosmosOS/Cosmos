using System;
using System.IO;


using CPU = Indy.IL2CPU.Assembler;
using CPUx86 = Indy.IL2CPU.Assembler.X86;
using System.Reflection;
using Indy.IL2CPU.Assembler;

namespace Indy.IL2CPU.IL.X86 {
	[OpCode(OpCodeEnum.Castclass, false)]
	public class Castclass: Op {
		private int mTypeId;
		private string mThisLabel;
		private string mNextOpLabel;
		private Type mCastAsType;
		private int mCurrentILOffset;
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
		}

		public override void DoAssemble() {
			// todo: throw an exception when the class does not support the cast!
			string mReturnNullLabel = mThisLabel + "_ReturnNull";
			new CPUx86.Pop(CPUx86.Registers.ECX);
			Assembler.StackContents.Pop();
			new CPUx86.Compare(CPUx86.Registers.ECX, "0");
			new CPUx86.JumpIfZero(mReturnNullLabel);
			new CPUx86.Pushd(CPUx86.Registers.AtECX);
			Assembler.StackContents.Push(new StackContent(4, true, false, false));
			new CPUx86.Pushd("0" + mTypeId + "h");
			Assembler.StackContents.Push(new StackContent(4, true, false, false));
			MethodBase xMethodIsInstance = Engine.GetMethodBase(Engine.GetType("", "Indy.IL2CPU.VTablesImpl"), "IsInstance", "System.Int32", "System.Int32");
			Engine.QueueMethod(xMethodIsInstance);
			Op xOp = new Call(xMethodIsInstance, mCurrentILOffset);
			xOp.Assembler = Assembler;
			xOp.Assemble();
			new CPUx86.Pop(CPUx86.Registers.EAX);
			Assembler.StackContents.Pop();
			new CPUx86.Compare(CPUx86.Registers.EAX, "0");
			new CPUx86.JumpIfEquals(mReturnNullLabel);
			new CPUx86.Pushd(CPUx86.Registers.ECX);
			new CPUx86.JumpAlways(mNextOpLabel);
			new CPU.Label(mReturnNullLabel);
			new CPUx86.Pushd("0");
			Assembler.StackContents.Push(new StackContent(4, mCastAsType));
		}
	}
}