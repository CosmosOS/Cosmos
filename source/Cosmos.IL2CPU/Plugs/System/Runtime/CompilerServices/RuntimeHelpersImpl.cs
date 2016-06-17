using System;

using Cosmos.Assembler;
using XSharp.Compiler;
using static XSharp.Compiler.XSRegisters;
using CPUx86 = Cosmos.Assembler.x86;

namespace Cosmos.IL2CPU.Plugs.System.Runtime.CompilerServices {
	[Plug(Target = typeof(global::System.Runtime.CompilerServices.RuntimeHelpers))]
	public static class RuntimeHelpersImpl {

		public static void cctor() {
			//TODO: do something
		}

		[Inline(TargetPlatform = TargetPlatform.x86)]
		public static void InitializeArray(Array array, RuntimeFieldHandle fldHandle) {
			// Arguments:
			//    Array aArray, RuntimeFieldHandle aFieldHandle
            XS.Set(OldToNewRegister(CPUx86.RegistersEnum.EDI), OldToNewRegister(CPUx86.RegistersEnum.EBP), sourceDisplacement: 0xC); // array
		    XS.Set(EDI, EDI, sourceIsIndirect: true);
            XS.Set(OldToNewRegister(CPUx86.RegistersEnum.ESI), OldToNewRegister(CPUx86.RegistersEnum.EBP), sourceDisplacement: 8);// aFieldHandle
            XS.Add(OldToNewRegister(CPUx86.RegistersEnum.EDI), 8);
		    XS.Push(EDI, isIndirect: true);
            XS.Add(OldToNewRegister(CPUx86.RegistersEnum.EDI), 4);
            XS.Set(EAX, EDI, sourceIsIndirect: true);
		    XS.Multiply(ESP, isIndirect: true, size: RegisterSize.Int32);
            XS.Pop(OldToNewRegister(CPUx86.RegistersEnum.ECX));
            XS.Set(OldToNewRegister(CPUx86.RegistersEnum.ECX), OldToNewRegister(CPUx86.RegistersEnum.EAX));
            XS.Set(OldToNewRegister(CPUx86.RegistersEnum.EAX), 0);
            XS.Add(OldToNewRegister(CPUx86.RegistersEnum.EDI), 4);

			XS.Label(".StartLoop");
			XS.Set(DL, ESI, sourceIsIndirect: true);
            XS.Set(EDI, DL, destinationIsIndirect: true);
			XS.Add(OldToNewRegister(CPUx86.RegistersEnum.EAX), 1);
            XS.Add(OldToNewRegister(CPUx86.RegistersEnum.ESI), 1);
            XS.Add(OldToNewRegister(CPUx86.RegistersEnum.EDI), 1);
			XS.Compare(OldToNewRegister(CPUx86.RegistersEnum.EAX), OldToNewRegister(CPUx86.RegistersEnum.ECX));
            XS.Jump(CPUx86.ConditionalTestEnum.Equal, ".EndLoop");
            XS.Jump(".StartLoop");

			XS.Label(".EndLoop");
		}
	}
}
