using System;
using CPUx86 = Cosmos.Assembler.x86;
using Cosmos.Assembler.x86;

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
              new CPUx86.x87.FloatLoad { DestinationReg = Registers.ESP, Size = 64, DestinationIsIndirect = true };
              new CPUx86.Add { SourceValue = 8, DestinationReg = Registers.ESP };
              new CPUx86.x87.FloatAdd { DestinationReg = CPUx86.Registers.ESP, DestinationIsIndirect = true, Size = 64 };
              new CPUx86.x87.FloatStoreAndPop { DestinationReg = Registers.ESP, Size = 64, DestinationIsIndirect = true };
            }
            else
            {
              new CPUx86.Pop { DestinationReg = CPUx86.Registers.EDX }; // low part
              new CPUx86.Pop { DestinationReg = CPUx86.Registers.EAX }; // high part
              new CPUx86.Add { DestinationReg = CPUx86.Registers.ESP, DestinationIsIndirect = true, SourceReg = CPUx86.Registers.EDX };
              new CPUx86.AddWithCarry { DestinationReg = CPUx86.Registers.ESP, DestinationIsIndirect = true, DestinationDisplacement = 4, SourceReg = CPUx86.Registers.EAX };
            }
          }
          else
          {
            if (xIsFloat) //float
            {
              new CPUx86.SSE.MoveSS { DestinationReg = CPUx86.Registers.XMM0, SourceReg = CPUx86.Registers.ESP, SourceIsIndirect = true }; 
              new CPUx86.Add { DestinationReg = CPUx86.Registers.ESP, SourceValue = 4 };
              new CPUx86.SSE.MoveSS { DestinationReg = CPUx86.Registers.XMM1, SourceReg = CPUx86.Registers.ESP, SourceIsIndirect = true }; 
              new CPUx86.SSE.AddSS { DestinationReg = CPUx86.Registers.XMM1, SourceReg = CPUx86.Registers.XMM0 };
              new CPUx86.SSE.MoveSS { DestinationReg = CPUx86.Registers.ESP, DestinationIsIndirect = true, SourceReg = CPUx86.Registers.XMM1 };
            }
            else //integer
            {
              new CPUx86.Pop { DestinationReg = CPUx86.Registers.EAX };
              new CPUx86.Add { DestinationReg = CPUx86.Registers.ESP, DestinationIsIndirect = true, SourceReg = CPUx86.Registers.EAX };
            }
          }
        }
      }
    }
}