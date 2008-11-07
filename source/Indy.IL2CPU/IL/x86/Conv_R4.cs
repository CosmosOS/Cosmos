using System;
using System.IO;


using CPUx86 = Indy.IL2CPU.Assembler.X86;

namespace Indy.IL2CPU.IL.X86 {
	[OpCode(OpCodeEnum.Conv_R4)]
	public class Conv_R4: Op {
		public Conv_R4(ILReader aReader, MethodInformation aMethodInfo)
			: base(aReader, aMethodInfo) {
		}
		public override void DoAssemble() {
			throw new Exception("Floats not yet supported!");
			//int xSource = Assembler.StackContents.Pop();
			//switch (xSource) {
			//    case 1:
			//    case 2: {
			//            break;
			//        }
			//    case 8: {
			//            new CPUx86.Pop(CPUx86.Registers_Old.EAX);
			//            new CPUx86.Pop(CPUx86.Registers_Old.ECX);
			//            new CPUx86.Pushd(CPUx86.Registers_Old.EAX);
			//            Assembler.StackContents.Push(4);
			//            break;
			//        }
			//    case 4: {
			//            break;
			//        }
			//    default:
			//        throw new Exception("SourceSize " + xSource + " not supported!");
			//}
		}
	}
}