using System;
using System.Linq;
using Indy.IL2CPU.Assembler.X86;


using CPU = Indy.IL2CPU.Assembler;
using CPUx86 = Indy.IL2CPU.Assembler.X86;
using Asm = Indy.IL2CPU.Assembler;
using Indy.IL2CPU.Assembler;
using System.Reflection;

namespace Indy.IL2CPU.IL.X86 {
	[OpCode(OpCodeEnum.Newobj, false)]
	public class Newobj: Op {
		public MethodBase CtorDef;
		public string CurrentLabel;
		public MethodInformation MethodInformation;
		public int ILOffset;
		public Newobj(ILReader aReader, MethodInformation aMethodInfo)
			: base(aReader, aMethodInfo) {
			CtorDef = aReader.OperandValueMethod;
			CurrentLabel = GetInstructionLabel(aReader);
			MethodInformation = aMethodInfo;
			ILOffset = (int)aReader.Position;
		}

		public override void DoAssemble() {
			Assemble(Assembler, CtorDef, Engine.RegisterType(CtorDef.DeclaringType), CurrentLabel, MethodInformation, ILOffset);
		}

		public static void Assemble(Assembler.Assembler aAssembler, MethodBase aCtorDef, int aTypeId, string aCurrentLabel, MethodInformation aCurrentMethodInformation, int aCurrentILOffset) {
			if (aCtorDef != null) {
				Engine.QueueMethod(aCtorDef);
			} else {
				throw new ArgumentNullException("aCtorDef");
			}
			int xObjectSize = ObjectUtilities.GetObjectStorageSize(aCtorDef.DeclaringType);
			MethodInformation xCtorInfo = Engine.GetMethodInfo(aCtorDef, aCtorDef, Label.GenerateLabelName(aCtorDef), Engine.GetTypeInfo(aCtorDef.DeclaringType), aCurrentMethodInformation.DebugMode);
			for (int i = 1; i < xCtorInfo.Arguments.Length; i++) {
				aAssembler.StackContents.Pop();
			}
			Engine.QueueMethod(GCImplementationRefs.AllocNewObjectRef);
			Engine.QueueMethod(GCImplementationRefs.IncRefCountRef);
			int xExtraSize = 16;
			if (!aAssembler.InMetalMode) {
				xExtraSize += 4;
			}
			new CPUx86.Pushd("0" + (xObjectSize + xExtraSize).ToString("X").ToUpper() + "h");
			new CPUx86.Call(CPU.Label.GenerateLabelName(GCImplementationRefs.AllocNewObjectRef));
			Engine.QueueMethod(CPU.Assembler.CurrentExceptionOccurredRef);
			//new CPUx86.Pushd(CPUx86.Registers.EAX);
			new CPUx86.Test(CPUx86.Registers.ECX, 2);
			//new CPUx86.JumpIfEquals(aCurrentLabel + "_NO_ERROR_1");
			//for (int i = 1; i < xCtorInfo.Arguments.Length; i++) {
			//    new CPUx86.Add(CPUx86.Registers.ESP, (xCtorInfo.Arguments[i].Size % 4 == 0 ? xCtorInfo.Arguments[i].Size : ((xCtorInfo.Arguments[i].Size / 4) * 4) + 1).ToString());
			//}
			//new CPUx86.Add("esp", "4");
			//Call.EmitExceptionLogic(aAssembler, aCurrentMethodInformation, aCurrentLabel + "_NO_ERROR_1", false);
			//new CPU.Label(aCurrentLabel + "_NO_ERROR_1");
			new CPUx86.Pushd(CPUx86.Registers.AtESP);
            new CPUx86.Pushd(CPUx86.Registers.AtESP);
            new CPUx86.Pushd(CPUx86.Registers.AtESP);
            new CPUx86.Pushd(CPUx86.Registers.AtESP);
			new CPUx86.Call(CPU.Label.GenerateLabelName(GCImplementationRefs.IncRefCountRef));
			//new CPUx86.Test("ecx", "2");
			//new CPUx86.JumpIfEquals(aCurrentLabel + "_NO_ERROR_2");
			//for (int i = 1; i < xCtorInfo.Arguments.Length; i++) {
			//    new CPUx86.Add(CPUx86.Registers.ESP, (xCtorInfo.Arguments[i].Size % 4 == 0 ? xCtorInfo.Arguments[i].Size : ((xCtorInfo.Arguments[i].Size / 4) * 4) + 1).ToString());
			//}
			//new CPUx86.Add("esp", "16");
			//Call.EmitExceptionLogic(aAssembler, aCurrentMethodInformation, aCurrentLabel + "_NO_ERROR_2", false);
			//new CPU.Label(aCurrentLabel + "_NO_ERROR_2");
			new CPUx86.Call(CPU.Label.GenerateLabelName(GCImplementationRefs.IncRefCountRef));
			//new CPUx86.Test("ecx", "2");
			//new CPUx86.JumpIfEquals(aCurrentLabel + "_NO_ERROR_3");
			//for (int i = 1; i < xCtorInfo.Arguments.Length; i++) {
			//    new CPUx86.Add(CPUx86.Registers.ESP, (xCtorInfo.Arguments[i].Size % 4 == 0 ? xCtorInfo.Arguments[i].Size : ((xCtorInfo.Arguments[i].Size / 4) * 4) + 1).ToString());
			//}
			//new CPUx86.Add("esp", "12");
			//Call.EmitExceptionLogic(aAssembler, aCurrentMethodInformation, aCurrentLabel + "_NO_ERROR_3", false);
			//new CPU.Label(aCurrentLabel + "_NO_ERROR_3");
			int xObjSize = 0;
			int xGCFieldCount = (from item in Engine.GetTypeFieldInfo(aCtorDef, out xObjSize).Values
								 where item.NeedsGC
								 select item).Count();
			new CPUx86.Pop(CPUx86.Registers.EAX);
			new Move("dword", CPUx86.Registers.AtEAX, "0" + aTypeId.ToString("X") + "h");
			new Move("dword", "[eax + 4]", "0" + InstanceTypeEnum.NormalObject.ToString("X") + "h");
			new Move("dword", "[eax + 8]", "0x" + xGCFieldCount.ToString("X"));
			int xSize = (from item in xCtorInfo.Arguments
						 select item.Size + (item.Size % 4 == 0 ? 0 : (4 - (item.Size % 4)))).Take(xCtorInfo.Arguments.Length - 1).Sum();
			for (int i = 1; i < xCtorInfo.Arguments.Length; i++) {
				new CPUx86.Pushd("[esp + 0x" + (xSize + 4).ToString("X") + "]");
			}
			new CPUx86.Call(CPU.Label.GenerateLabelName(aCtorDef));
			new CPUx86.Test(CPUx86.Registers.ECX, 2);
			new CPUx86.JumpIfEqual(aCurrentLabel + "_NO_ERROR_4");
			for (int i = 1; i < xCtorInfo.Arguments.Length; i++) {
				new CPUx86.Add(CPUx86.Registers.ESP, (xCtorInfo.Arguments[i].Size % 4 == 0 ? xCtorInfo.Arguments[i].Size : ((xCtorInfo.Arguments[i].Size / 4) * 4) + 1).ToString());
			}
			new CPUx86.Add(CPUx86.Registers.ESP, "4");
			foreach (var xStackInt in aAssembler.StackContents) {
                new CPUx86.Add(CPUx86.Registers.ESP, xStackInt.Size.ToString());
			}
			Call.EmitExceptionLogic(aAssembler, aCurrentILOffset, aCurrentMethodInformation, aCurrentLabel + "_NO_ERROR_4", false, null);
			new CPU.Label(aCurrentLabel + "_NO_ERROR_4");
			new CPUx86.Pop(CPUx86.Registers.EAX);
			//				aAssembler.StackSizes.Pop();
			//	new CPUx86.Add(CPUx86.Registers.ESP, "4");
			for (int i = 1; i < xCtorInfo.Arguments.Length; i++) {
				new CPUx86.Add(CPUx86.Registers.ESP, (xCtorInfo.Arguments[i].Size % 4 == 0 ? xCtorInfo.Arguments[i].Size : ((xCtorInfo.Arguments[i].Size / 4) * 4) + 1).ToString());
			}
			new CPUx86.Push(CPUx86.Registers.EAX);
			aAssembler.StackContents.Push(new StackContent(4, aCtorDef.DeclaringType));
		}
	}
}