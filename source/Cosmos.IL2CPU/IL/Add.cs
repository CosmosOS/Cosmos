using System;
using CPUx86 = Cosmos.Assembler.x86;
using Cosmos.Assembler.x86;
using Cosmos.Assembler.x86.SSE;
using XSharp.Compiler;

namespace Cosmos.IL2CPU.X86.IL
{
    [Cosmos.IL2CPU.OpCode( ILOpCode.Code.Add )]
    public class Add : ILOp
    {
        public Add( Cosmos.Assembler.Assembler aAsmblr )
            : base( aAsmblr )
        {
        }

        public override void Execute( MethodInfo aMethod, ILOpCode aOpCode )
        {
            var xType = aOpCode.StackPopTypes[0];
            var xSize = SizeOfType(xType);
            var xIsFloat = TypeIsFloat(xType);
            DoExecute(xSize, xIsFloat);
        }

      public static void DoExecute(uint xSize, bool xIsFloat)
      {
        if (xSize > 8)
        {
          //EmitNotImplementedException( Assembler, aServiceProvider, "Size '" + xSize.Size + "' not supported (add)", aCurrentLabel, aCurrentMethodInfo, aCurrentOffset, aNextLabel );
          throw new NotImplementedException("Cosmos.IL2CPU.x86->IL->Add.cs->Error: StackSize > 8 not supported");
        }
        else
        {
          if (xSize > 4)
          {
            if (xIsFloat)
            {
              new CPUx86.x87.FloatLoad { DestinationReg = RegistersEnum.ESP, Size = 64, DestinationIsIndirect = true };
              new CPUx86.Add { SourceValue = 8, DestinationReg = RegistersEnum.ESP };
              new CPUx86.x87.FloatAdd { DestinationReg = CPUx86.RegistersEnum.ESP, DestinationIsIndirect = true, Size = 64 };
              new CPUx86.x87.FloatStoreAndPop { DestinationReg = RegistersEnum.ESP, Size = 64, DestinationIsIndirect = true };
            }
            else
            {
              XS.Pop(XSRegisters.OldToNewRegister(RegistersEnum.EDX)); // low part
              XS.Pop(XSRegisters.OldToNewRegister(RegistersEnum.EAX)); // high part
              new CPUx86.Add { DestinationReg = RegistersEnum.ESP, DestinationIsIndirect = true, SourceReg = RegistersEnum.EDX };
              new AddWithCarry { DestinationReg = RegistersEnum.ESP, DestinationIsIndirect = true, DestinationDisplacement = 4, SourceReg = RegistersEnum.EAX };
            }
          }
          else
          {
            if (xIsFloat) //float
            {
              new MoveSS { DestinationReg = RegistersEnum.XMM0, SourceReg = RegistersEnum.ESP, SourceIsIndirect = true };
              new CPUx86.Add { DestinationReg = RegistersEnum.ESP, SourceValue = 4 };
              new MoveSS { DestinationReg = RegistersEnum.XMM1, SourceReg = RegistersEnum.ESP, SourceIsIndirect = true };
              new AddSS { DestinationReg = RegistersEnum.XMM1, SourceReg = RegistersEnum.XMM0 };
              new MoveSS { DestinationReg = RegistersEnum.ESP, DestinationIsIndirect = true, SourceReg = RegistersEnum.XMM1 };
            }
            else //integer
            {
              XS.Pop(XSRegisters.OldToNewRegister(RegistersEnum.EAX));
              new CPUx86.Add { DestinationReg = CPUx86.RegistersEnum.ESP, DestinationIsIndirect = true, SourceReg = CPUx86.RegistersEnum.EAX };
            }
          }
        }
      }
    }
}
