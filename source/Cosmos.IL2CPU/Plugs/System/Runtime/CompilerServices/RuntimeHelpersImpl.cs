using System;

using Cosmos.Assembler;
using XSharp.Compiler;
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
            XS.Set(XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.EDI), XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.EBP), sourceDisplacement: 0xC); // array
		    XS.Set(XSRegisters.EDI, XSRegisters.EDI, sourceIsIndirect: true);
            XS.Set(XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.ESI), XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.EBP), sourceDisplacement: 8);// aFieldHandle
            XS.Add(XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.EDI), 8);
		    XS.Push(XSRegisters.EDI, isIndirect: true);
            XS.Add(XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.EDI), 4);
            XS.Set(XSRegisters.EAX, XSRegisters.EDI, sourceIsIndirect: true);
		    new CPUx86.Multiply {DestinationReg = CPUx86.RegistersEnum.ESP, DestinationIsIndirect = true, Size = 32};
            XS.Pop(XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.ECX));
            XS.Set(XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.ECX), XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.EAX));
            XS.Set(XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.EAX), 0);
            XS.Add(XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.EDI), 4);

			XS.Label(".StartLoop");
			XS.Set(XSRegisters.DL, XSRegisters.ESI, sourceIsIndirect: true);
            XS.Set(XSRegisters.EDI, XSRegisters.DL, destinationIsIndirect: true);
			XS.Add(XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.EAX), 1);
            XS.Add(XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.ESI), 1);
            XS.Add(XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.EDI), 1);
			XS.Compare(XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.EAX), XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.ECX));
            XS.Jump(CPUx86.ConditionalTestEnum.Equal, ".EndLoop");
            XS.Jump(".StartLoop");

			XS.Label(".EndLoop");
		}
	}
}
