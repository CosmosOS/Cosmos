using System;
using System.Linq;
using CPUx86 = Cosmos.Assembler.x86;
using Cosmos.Assembler;
using Cosmos.IL2CPU.ILOpCodes;
using Cosmos.IL2CPU.Plugs.System;
using XSharp.Compiler;
using static XSharp.Compiler.XSRegisters;

namespace Cosmos.IL2CPU.X86.IL
{
  [Cosmos.IL2CPU.OpCode(ILOpCode.Code.Ldelema)]
  public class Ldelema : ILOp
  {
    public Ldelema(Cosmos.Assembler.Assembler aAsmblr)
      : base(aAsmblr)
    {
    }

    public static void Assemble(Cosmos.Assembler.Assembler aAssembler, OpType aOpType, uint aElementSize, bool debugEnabled)
    {
      XS.Comment("Arraytype: " + aOpType.StackPopTypes.Last().FullName);
      XS.Comment("Size: " + aElementSize);

      DoNullReferenceCheck(aAssembler, debugEnabled, 8);
      // calculate element offset into array memory (including header)
      XS.Pop(EAX);
      XS.Set(EDX, aElementSize);
      XS.Multiply(EDX);
      XS.Add(EAX, (uint) (ObjectImpl.FieldDataOffset + 4));

      // pop the array now
      XS.Add(ESP, 4);
      XS.Pop(EDX);

      XS.Add(EDX, EAX);
      XS.Push(EDX);
    }

    public override void Execute(MethodInfo aMethod, ILOpCode aOpCode)
    {
      var xOpType = (OpType) aOpCode;
      var xSize = SizeOfType(xOpType.Value);
      Assemble(Assembler, xOpType, xSize, DebugEnabled);
    }


    // using System;
    // using System.IO;
    //
    //
    // using CPU = Cosmos.Assembler.x86;
    // using CPUx86 = Cosmos.Assembler.x86;
    // using Cosmos.IL2CPU.X86;
    // using Cosmos.IL2CPU.Compiler;
    //
    // namespace Cosmos.IL2CPU.IL.X86 {
    // 	[Cosmos.Assembler.OpCode(OpCodeEnum.Ldelema)]
    // 	public class Ldelema: Op {
    //         private Type mType;
    // 		public Ldelema(ILReader aReader, MethodInformation aMethodInfo)
    // 			: base(aReader, aMethodInfo) {
    // 			mType= aReader.OperandValueType;
    // 		}
    //
    // 		public static void Assemble(CPU.Assembler aAssembler, uint aElementSize) {
    // 			aAssembler.Stack.Pop();
    // 			aAssembler.Stack.Pop();
    // 			aAssembler.Stack.Push(new StackContent(4, typeof(uint)));
    //             XS.Pop(XSRegisters.EAX);
    //             XS.Mov(XSRegisters.EDX, aElementSize);
    // 			XS.Multiply(XSRegisters.EDX);
    //             new CPUx86.Add { DestinationReg = CPUx86.Registers.EAX, SourceValue = (uint)(ObjectImpl.FieldDataOffset + 4) };
    //             XS.Pop(XSRegisters.EDX);
    //             XS.Add(XSRegisters.EDX, XSRegisters.CPUx86.Registers.EAX);
    //             XS.Push(XSRegisters.EDX);
    // 		}
    //
    // 		public override void DoAssemble() {
    //             var xElementSize = GetService<IMetaDataInfoService>().SizeOfType(mType);
    // 			Assemble(Assembler, xElementSize);
    // 		}
    // 	}
    // }

  }
}
