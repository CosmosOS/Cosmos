using System;
using CPU = Cosmos.Assembler.x86;
using Cosmos.IL2CPU.ILOpCodes;
using Cosmos.Assembler;
using XSharp.Compiler;

namespace Cosmos.IL2CPU.X86.IL
{
  [Cosmos.IL2CPU.OpCode(ILOpCode.Code.Ldc_R8)]
  public class Ldc_R8 : ILOp
  {
    public Ldc_R8(Cosmos.Assembler.Assembler aAsmblr)
        : base(aAsmblr)
    {
    }

    public override void Execute(MethodInfo aMethod, ILOpCode aOpCode)
    {
      OpDouble xOp = (OpDouble)aOpCode;
      byte[] xBytes = BitConverter.GetBytes(xOp.Value);
      new CPU.Push { DestinationValue = BitConverter.ToUInt32(xBytes, 4) };
      new CPU.Push { DestinationValue = BitConverter.ToUInt32(xBytes, 0) };
    }


    // using System;
    // using System.Linq;
    //
    // using CPU = Cosmos.Assembler.x86;
    // using Cosmos.IL2CPU.X86;
    //
    // namespace Cosmos.IL2CPU.IL.X86 {
    // 	[Cosmos.Assembler.OpCode(OpCodeEnum.Ldc_R8)]
    // 	public class Ldc_R8: Op {
    // 		private readonly Double mValue;
    // 		public Ldc_R8(ILReader aReader, MethodInformation aMethodInfo)
    // 			: base(aReader, aMethodInfo) {
    // 			mValue = aReader.OperandValueDouble;
    // 		}
    // 		public override void DoAssemble() {
    // 			byte[] xBytes = BitConverter.GetBytes(mValue);
    //             new CPU.Push { DestinationValue = BitConverter.ToUInt32(xBytes, 0) };
    //             new CPU.Push { DestinationValue = BitConverter.ToUInt32(xBytes, 4) };
    // 			Assembler.Stack.Push(new StackContent(8, typeof(Double)));
    // 		}
    // 	}
    // }

  }
}
