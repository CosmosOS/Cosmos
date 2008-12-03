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
		private uint mTypeSize;
		private uint mCurrentILOffset;
		private bool mDebugMode;
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
			mDebugMode = aMethodInfo.DebugMode;
		}
		public override void DoAssemble() {
			string mReturnNullLabel = mThisLabel + "_ReturnNull";
            new CPUx86.Move { DestinationReg = CPUx86.Registers.EAX, SourceReg = CPUx86.Registers.ESP, SourceIsIndirect = true };
            new CPUx86.Compare { DestinationReg = CPUx86.Registers.EAX, SourceValue = 0 };
            new CPUx86.ConditionalJump { Condition = CPUx86.ConditionalTestEnum.Zero, DestinationLabel = mReturnNullLabel };
            new CPUx86.Push { DestinationReg = CPUx86.Registers.EAX, DestinationIsIndirect = true };
			Assembler.StackContents.Push(new StackContent(4, typeof(uint)));
            new CPUx86.Push { DestinationValue = (uint)mTypeId };
			Assembler.StackContents.Push(new StackContent(4, typeof(uint)));
			MethodBase xMethodIsInstance = Engine.GetMethodBase(Engine.GetType("", "Indy.IL2CPU.VTablesImpl"), "IsInstance", "System.Int32", "System.Int32");
			Engine.QueueMethod(xMethodIsInstance);
			Op xOp = new Call(xMethodIsInstance, mCurrentILOffset, mDebugMode, mThisLabel + "_After_IsInstance_Call");
			xOp.Assembler = Assembler;
			xOp.Assemble();
		    new Label(mThisLabel + "_After_IsInstance_Call");
            new CPUx86.Pop { DestinationReg = CPUx86.Registers.EAX };
			Assembler.StackContents.Pop();
            new CPUx86.Compare { DestinationReg = CPUx86.Registers.EAX, SourceValue = 0 };
            new CPUx86.ConditionalJump { Condition = CPUx86.ConditionalTestEnum.Equal, DestinationLabel = mReturnNullLabel };
            new CPUx86.Pop { DestinationReg = CPUx86.Registers.EAX };
			uint xSize = mTypeSize;
			if (xSize % 4 > 0) {
				xSize += 4 - (xSize % 4);
			}
			int xItems =(int) xSize /4;
			for (int i = xItems - 1; i >= 0; i--) {
                new CPUx86.Push { DestinationReg = CPUx86.Registers.EAX, DestinationIsIndirect = true, DestinationDisplacement = ((i * 4) + ObjectImpl.FieldDataOffset) };
			}
			Assembler.StackContents.Push(new StackContent((int)mTypeSize, mType));
            new CPUx86.Jump { DestinationLabel = mNextOpLabel };
			new CPU.Label(mReturnNullLabel);
            new CPUx86.Add { DestinationReg = CPUx86.Registers.ESP, SourceValue = 4 };
            new CPUx86.Push { DestinationValue = 0 };
			Assembler.StackContents.Push(new StackContent(4, typeof(object)));
		}
	}
}