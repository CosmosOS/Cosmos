using System;
using CPUx86 = Cosmos.Assembler.x86;
using Cosmos.Assembler;
using Cosmos.IL2CPU.IL.CustomImplementations.System;
namespace Cosmos.IL2CPU.X86.IL
{
    [Cosmos.IL2CPU.OpCode( ILOpCode.Code.Ldelem_Ref )]
    public class Ldelem_Ref : ILOp
    {
        public Ldelem_Ref( Cosmos.Assembler.Assembler aAsmblr )
            : base( aAsmblr )
        {
        }
        public static void Assemble(Cosmos.Assembler.Assembler aAssembler,  uint aElementSize,bool isSigned , bool debugEnabled)
        {
          DoNullReferenceCheck(aAssembler, debugEnabled, 4);
			if (aElementSize <= 0 || aElementSize > 8 || (aElementSize > 4 && aElementSize < 8))
				throw new Exception("Unsupported size for Ldelem_Ref: " + aElementSize);

            new CPUx86.Pop { DestinationReg = CPUx86.Registers.EAX };
            new CPUx86.Mov { DestinationReg = CPUx86.Registers.EDX, SourceValue = aElementSize };
            new CPUx86.Multiply { DestinationReg = CPUx86.Registers.EDX };

            //TODO: implement ObjectImpl first
            new CPUx86.Add { DestinationReg = CPUx86.Registers.EAX, SourceValue = ( ObjectImpl.FieldDataOffset + 4 ) };

            new CPUx86.Pop { DestinationReg = CPUx86.Registers.EDX };
            new CPUx86.Add { DestinationReg = CPUx86.Registers.EDX, SourceReg = CPUx86.Registers.EAX };

			switch (aElementSize)
			{
				case 1:
					if (isSigned)
						new CPUx86.MoveSignExtend { DestinationReg = CPUx86.Registers.ECX, Size = 8, SourceReg = CPUx86.Registers.EDX, SourceIsIndirect = true };
					else
						new CPUx86.MoveZeroExtend { DestinationReg = CPUx86.Registers.ECX, Size = 8, SourceReg = CPUx86.Registers.EDX, SourceIsIndirect = true };
					new CPUx86.Push { DestinationReg = CPUx86.Registers.ECX };
					break;
				case 2:
					if (isSigned)
						new CPUx86.MoveSignExtend { DestinationReg = CPUx86.Registers.ECX, Size = 16, SourceReg = CPUx86.Registers.EDX, SourceIsIndirect = true };
					else
						new CPUx86.MoveZeroExtend { DestinationReg = CPUx86.Registers.ECX, Size = 16, SourceReg = CPUx86.Registers.EDX, SourceIsIndirect = true };
					new CPUx86.Push { DestinationReg = CPUx86.Registers.ECX };
					break;
				case 4:
					new CPUx86.Push { DestinationReg = CPUx86.Registers.EDX, DestinationIsIndirect = true };
					break;
				case 8:
                    new CPUx86.Push { DestinationReg = CPUx86.Registers.EDX, DestinationDisplacement = 4, DestinationIsIndirect = true };
					new CPUx86.Push { DestinationReg = CPUx86.Registers.EDX, DestinationIsIndirect = true };
					break;
			}
        }

        public override void Execute( MethodInfo aMethod, ILOpCode aOpCode )
        {
            Assemble( Assembler, 4, false, DebugEnabled);
        }
    }
}