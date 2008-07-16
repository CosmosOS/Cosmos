using System;
using System.Collections.Generic;
using CPU = Indy.IL2CPU.Assembler;
using CPUx86 = Indy.IL2CPU.Assembler.X86;
using System.Reflection;
using Indy.IL2CPU.Assembler;

namespace Indy.IL2CPU.IL.X86 {
	[OpCode(OpCodeEnum.Newarr, false)]
	public class Newarr: Op {
		private int mElementSize;
		private string mCtorName;
        public static void ScanOp(ILReader aReader, MethodInformation aMethodInfo, SortedList<string, object> aMethodData) {
            Type xTypeRef = aReader.OperandValueType;
            if (xTypeRef == null)
            {
                throw new Exception("No TypeRef found!");
            }
            Engine.RegisterType(xTypeRef);
            Type xArrayType = Engine.GetType("mscorlib", "System.Array");
            Engine.RegisterType(xArrayType);
            MethodBase xCtor = xArrayType.GetConstructors(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance)[0];
            Engine.QueueMethod(xCtor);
        }

		public Newarr(Type aTypeRef, string aBaseLabelName)
			: base(null, null) {
			Initialize(aTypeRef, aBaseLabelName);
		}

		public Newarr(ILReader aReader, MethodInformation aMethodInfo)
			: base(aReader, aMethodInfo) {
			Type xTypeRef = aReader.OperandValueType;
			if (xTypeRef == null) {
				throw new Exception("No TypeRef found!");
			}
			Initialize(xTypeRef, GetInstructionLabel(aReader));
		}

		private void Initialize(Type aTypeRef, string aBaseLabelName) {
			Type xTypeDef = aTypeRef;
			mElementSize = Engine.GetFieldStorageSize(aTypeRef);
			Type xArrayType = Engine.GetType("mscorlib", "System.Array");
			MethodBase xCtor = xArrayType.GetConstructors(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance)[0];
			mCtorName = CPU.Label.GenerateLabelName(xCtor);
			Engine.QueueMethod(xCtor);
			DoQueueMethod(GCImplementationRefs.AllocNewObjectRef);
		}

		public override void DoAssemble() {
			new CPU.Comment("Element Size = " + mElementSize);
			// element count is on the stack
			int xElementCountSize = Assembler.StackContents.Pop().Size;
			new CPUx86.Pop(CPUx86.Registers.ESI);
			new CPUx86.Pushd(CPUx86.Registers.ESI);
			//Assembler.StackSizes.Push(xElementCountSize);
			new CPUx86.Pushd("0x" + mElementSize.ToString("X"));
			Assembler.StackContents.Push(new StackContent(4, typeof(uint)));
			Multiply(Assembler);
			// the total items size is now on the stack
			new CPUx86.Pushd("0x" + (ObjectImpl.FieldDataOffset + 4).ToString("X"));
			Assembler.StackContents.Push(new StackContent(4, typeof(uint)));
			Add(Assembler);
			// the total array size is now on the stack.
			Engine.QueueMethod(GCImplementationRefs.AllocNewObjectRef);
			new CPUx86.Call(CPU.Label.GenerateLabelName(GCImplementationRefs.AllocNewObjectRef));
			new CPUx86.Pushd(CPUx86.Registers.AtESP);
            new CPUx86.Pushd(CPUx86.Registers.AtESP);
            new CPUx86.Pushd(CPUx86.Registers.AtESP);
            new CPUx86.Pushd(CPUx86.Registers.AtESP);
			//new CPUx86.Pushd(CPUx86.Registers.EDI);
			Engine.QueueMethod(GCImplementationRefs.IncRefCountRef);
			new CPUx86.Call(CPU.Label.GenerateLabelName(GCImplementationRefs.IncRefCountRef));
			new CPUx86.Call(CPU.Label.GenerateLabelName(GCImplementationRefs.IncRefCountRef));
			//new CPUx86.Popd(CPUx86.Registers.ESI);
			Assembler.StackContents.Push(new StackContent(4, typeof(Array)));
			new CPUx86.Pop(CPUx86.Registers.EAX);
			new CPUx86.Move("dword", CPUx86.Registers.AtEAX, "0x" + Engine.RegisterType(Engine.GetType("mscorlib", "System.Array")).ToString("X"));
			new CPUx86.Add(CPUx86.Registers.EAX, "4");
			new CPUx86.Move("dword", CPUx86.Registers.AtEAX, "0x" + InstanceTypeEnum.Array.ToString("X"));
			new CPUx86.Add(CPUx86.Registers.EAX, "4");
			new CPUx86.Move("dword", CPUx86.Registers.AtEAX, CPUx86.Registers.ESI);
			new CPUx86.Add(CPUx86.Registers.EAX, "4");
			new CPUx86.Move("dword", CPUx86.Registers.AtEAX, "0x" + mElementSize.ToString("X"));
			new CPUx86.Call(mCtorName);
		}
	}
}