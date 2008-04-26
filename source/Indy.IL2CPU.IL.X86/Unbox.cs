using System;
using System.IO;


using CPU = Indy.IL2CPU.Assembler;
using CPUx86 = Indy.IL2CPU.Assembler.X86;
using System.Reflection;
using Indy.IL2CPU.Assembler;

namespace Indy.IL2CPU.IL.X86 {
	[OpCode(OpCodeEnum.Unbox)]
	public class Unbox: Op {
		private int mTypeId;
		private string mThisLabel;
		private string mNextOpLabel;
		private Type mType;
		private int mTypeSize;
		private int mCurrentILOffset;
		public Unbox(ILReader aReader, MethodInformation aMethodInfo)
			: base(aReader, aMethodInfo) {
			mType = aReader.OperandValueType;
			if (mType == null) {
				throw new Exception("Unable to determine Type!");
			}
			mTypeSize = Engine.GetFieldStorageSize(mType);
			mTypeId = Engine.RegisterType(mType);
			mThisLabel = GetInstructionLabel(aReader);
			mNextOpLabel = GetInstructionLabel(aReader.NextPosition);
			mCurrentILOffset = aReader.Position;
		}
		public override void DoAssemble() {
			string mReturnNullLabel = mThisLabel + "_ReturnNull";
			new CPUx86.Move(CPUx86.Registers.EAX, CPUx86.Registers.AtESP);
			new CPUx86.Compare(CPUx86.Registers.EAX, "0");
			new CPUx86.JumpIfZero(mReturnNullLabel);
			new CPUx86.Pushd(CPUx86.Registers.AtEAX);
			Assembler.StackContents.Push(new StackContent(4, typeof(uint)));
			new CPUx86.Pushd("0" + mTypeId + "h");
			Assembler.StackContents.Push(new StackContent(4, typeof(uint)));
			MethodBase xMethodIsInstance = Engine.GetMethodBase(Engine.GetType("", "Indy.IL2CPU.VTablesImpl"), "IsInstance", "System.Int32", "System.Int32");
			Engine.QueueMethod(xMethodIsInstance);
			Op xOp = new Call(xMethodIsInstance, mCurrentILOffset);
			xOp.Assembler = Assembler;
			xOp.Assemble();
			new CPUx86.Pop(CPUx86.Registers.EAX);
			Assembler.StackContents.Pop();
			new CPUx86.Compare(CPUx86.Registers.EAX, "0");
			new CPUx86.JumpIfEquals(mReturnNullLabel);
			new CPUx86.Pop(CPUx86.Registers.EAX);
			int xSize = mTypeSize;
			if (xSize % 4 > 0) {
				xSize += 4 - (xSize % 4);
			}
			int xItems = xSize /4;
			for (int i = xItems - 1; i >= 0; i--) {
				new CPUx86.Pushd("[eax + " + ((i * 4) + ObjectImpl.FieldDataOffset) + "]");
			}
			Assembler.StackContents.Push(new StackContent(mTypeSize, mType));
			new CPUx86.Jump(mNextOpLabel);
			new CPU.Label(mReturnNullLabel);
			new CPUx86.Add(CPUx86.Registers.ESP, "4");
			new CPUx86.Pushd("0");
			Assembler.StackContents.Push(new StackContent(4, typeof(object)));
		}
	}
}