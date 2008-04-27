using System;
using System.IO;


using CPU = Indy.IL2CPU.Assembler;
using CPUx86 = Indy.IL2CPU.Assembler.X86;
using System.Reflection;
using Indy.IL2CPU.Assembler;

namespace Indy.IL2CPU.IL.X86 {
	[OpCode(OpCodeEnum.Isinst)]
	public class Isinst: Op {
		private int mTypeId;
		private string mThisLabel;
		private string mNextOpLabel;
		private int mCurrentILOffset;
		public Isinst(ILReader aReader, MethodInformation aMethodInfo)
			: base(aReader, aMethodInfo) {
			Type xType = aReader.OperandValueType;
			if (xType == null) {
				throw new Exception("Unable to determine Type!");
			}
			Type xTypeDef = xType;
			mTypeId = Engine.RegisterType(xTypeDef);
			mThisLabel = GetInstructionLabel(aReader.Position);
			mNextOpLabel = GetInstructionLabel(aReader.NextPosition);
			mCurrentILOffset = (int)aReader.Position;
		}

		public override void DoAssemble() {
			string mReturnNullLabel = mThisLabel + "_ReturnNull";
			new CPUx86.Move(CPUx86.Registers.EAX, CPUx86.Registers.AtESP);
			new CPUx86.Compare(CPUx86.Registers.EAX, "0");
			new CPUx86.JumpIfZero(mReturnNullLabel);
			new CPUx86.Pushd(CPUx86.Registers.AtEAX);
			new CPUx86.Pushd("0" + mTypeId );
			Assembler.StackContents.Push(new StackContent(4, typeof(object)));
			Assembler.StackContents.Push(new StackContent(4, typeof(object)));
			MethodBase xMethodIsInstance = Engine.GetMethodBase(typeof(VTablesImpl), "IsInstance", "System.Int32", "System.Int32");
			Engine.QueueMethod(xMethodIsInstance);
			Op xOp = new Call(xMethodIsInstance, mCurrentILOffset);
			xOp.Assembler = Assembler;
			xOp.Assemble();
			Assembler.StackContents.Pop();
			new CPUx86.Pop(CPUx86.Registers.EAX);
			new CPUx86.Compare(CPUx86.Registers.EAX, "0");
			new CPUx86.JumpIfNotEqual(mNextOpLabel);
			new CPU.Label(mReturnNullLabel);
			new CPUx86.Add("esp", "4");
			new CPUx86.Pushd("0");
		}
	}
}