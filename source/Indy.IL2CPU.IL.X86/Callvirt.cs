using System;
using System.Linq;


using CPU = Indy.IL2CPU.Assembler;
using CPUx86 = Indy.IL2CPU.Assembler.X86;
using System.Reflection;
using Indy.IL2CPU.Assembler;

namespace Indy.IL2CPU.IL.X86 {
	[OpCode(OpCodeEnum.Callvirt, true)]
	public class Callvirt: Op {
		private readonly int mMethodIdentifier;
		private readonly string mNormalAddress;
		private readonly string mMethodDescription;
		private readonly int mThisOffset;
		private readonly int mArgumentCount;
		private readonly int mReturnSize;
		private readonly string mLabelName;
		private readonly MethodInformation mMethodInfo;
		public Callvirt(ILReader aReader, MethodInformation aMethodInfo)
			: base(aReader, aMethodInfo) {
			mLabelName = GetInstructionLabel(aReader);
			mMethodInfo = aMethodInfo;
			int xThisOffSet = (from item in aMethodInfo.Locals
							   select item.Offset + item.Size).LastOrDefault();
			MethodBase xMethod = aReader.OperandValueMethod;
			if (xMethod == null) {
				throw new Exception("Unable to determine Method!");
			}
			MethodBase xMethodDef = xMethod;
			mMethodDescription = CPU.Label.GenerateLabelName(xMethodDef);
			MethodInformation xTheMethodInfo = Engine.GetMethodInfo(xMethodDef, xMethodDef, mMethodDescription, null);
			if (xMethodDef.IsStatic || !xMethodDef.IsVirtual) {
				Engine.QueueMethod(xMethodDef);
				mNormalAddress = CPU.Label.GenerateLabelName(xMethodDef);
				mReturnSize = xTheMethodInfo.ReturnSize;
				return;
			}
			mMethodIdentifier = Engine.GetMethodIdentifier(xMethodDef);
			Engine.QueueMethod(VTablesImplRefs.GetMethodAddressForTypeRef);
			mArgumentCount = xTheMethodInfo.Arguments.Length;
			mReturnSize = xTheMethodInfo.ReturnSize;
			mThisOffset = xTheMethodInfo.Arguments[0].Offset;
		}

		public override void DoAssemble() {
			Action xEmitCleanup = delegate() {
				foreach (MethodInformation.Argument xArg in mMethodInfo.Arguments) {
					new CPUx86.Add("esp", xArg.Size.ToString());
				}
			};
			if (!String.IsNullOrEmpty(mNormalAddress)) {
				new CPUx86.Call(mNormalAddress);
			} else {
				if (Assembler.InMetalMode) {
					throw new Exception("Virtual methods not supported in Metal mode! (Called method = '" + mMethodDescription + "')");
				}
				//Assembler.Add(new CPUx86.Pop("eax"));
				//Assembler.Add(new CPUx86.Pushd("eax"));
				EmitCompareWithNull(Assembler, mMethodInfo, "[esp + 0x" + mThisOffset.ToString("X") + "]", mLabelName, mLabelName + "_AfterNullRefCheck", xEmitCleanup);
				new CPU.Label(mLabelName + "_AfterNullRefCheck");
				new CPUx86.Move(CPUx86.Registers.EAX, "[esp + 0x" + mThisOffset.ToString("X") + "]");
				new CPUx86.Pushd(CPUx86.Registers.AtEAX);
				new CPUx86.Pushd("0" + mMethodIdentifier.ToString("X") + "h");
				new CPUx86.Call(CPU.Label.GenerateLabelName(VTablesImplRefs.GetMethodAddressForTypeRef));
				Call.EmitExceptionLogic(Assembler, mMethodInfo, mLabelName + "_AfterAddressCheck", true);
				new CPU.Label(mLabelName + "_AfterAddressCheck");
				new CPUx86.Call(CPUx86.Registers.EAX);
			}
			if (!Assembler.InMetalMode) {
				new CPUx86.Test("ecx", "2");
				new CPUx86.JumpIfNotEquals(MethodFooterOp.EndOfMethodLabelNameException);
			}
			new CPU.Comment("Argument Count = " + mArgumentCount.ToString());
			for (int i = 0; i < mArgumentCount; i++) {
				Assembler.StackContents.Pop();
			}
			if (mReturnSize == 0) {
				return;
			}
			if (mReturnSize <= 4) {
				new CPUx86.Pushd(CPUx86.Registers.EAX);
				Assembler.StackContents.Push(new StackContent(mReturnSize, mMethodInfo.ReturnType));
				return;
			}
			if (mReturnSize <= 8) {
				new CPUx86.Pushd(CPUx86.Registers.EBX);
				new CPUx86.Pushd(CPUx86.Registers.EAX);
				Assembler.StackContents.Push(new StackContent(mReturnSize, mMethodInfo.ReturnType));
				return;
			}
		}
	}
}