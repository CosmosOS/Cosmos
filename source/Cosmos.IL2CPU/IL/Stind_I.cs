using System;
using CPUx86 = Cosmos.Assembler.x86;
using Cosmos.Assembler;
namespace Cosmos.IL2CPU.X86.IL
{
    [Cosmos.IL2CPU.OpCode( ILOpCode.Code.Stind_I )]
    public class Stind_I : ILOp
    {
        public Stind_I( Cosmos.Assembler.Assembler aAsmblr )
            : base( aAsmblr )
        {
        }

        public static void Assemble(Cosmos.Assembler.Assembler aAssembler,  int aSize, bool debugEnabled)
        {
            DoNullReferenceCheck(aAssembler, debugEnabled, Align((uint)aSize, 4));
            new Comment(aAssembler,  "address at: [esp + " + aSize + "]" );
            int xStorageSize = aSize;
            if( xStorageSize < 4 )
            {
                xStorageSize = 4;
            }
            new CPUx86.Mov { DestinationReg = CPUx86.Registers.EBX, SourceReg = CPUx86.Registers.ESP, SourceIsIndirect = true, SourceDisplacement = xStorageSize };
            for( int i = 0; i < ( aSize / 4 ); i++ )
            {
                new CPUx86.Mov { DestinationReg = CPUx86.Registers.EAX, SourceReg = CPUx86.Registers.ESP, SourceIsIndirect = true, SourceDisplacement = i * 4 };
                new CPUx86.Mov { DestinationReg = CPUx86.Registers.EBX, DestinationIsIndirect = true, DestinationDisplacement = i * 4, SourceReg = CPUx86.Registers.EAX };
            }
            switch( aSize % 4 )
            {
                case 0:
                    {
                        break;
                    }
                case 1:
                    {
                        new CPUx86.Mov { DestinationReg = CPUx86.Registers.EAX, SourceReg = CPUx86.Registers.ESP, SourceIsIndirect = true, SourceDisplacement = ( ( aSize / 4 ) * 4 ) };
                        new CPUx86.Mov { DestinationReg = CPUx86.Registers.EBX, DestinationIsIndirect = true, SourceDisplacement = ( ( aSize / 4 ) * 4 ), SourceReg = CPUx86.Registers.AL };
                        break;
                    }
                case 2:
                    {
                        new CPUx86.Mov { DestinationReg = CPUx86.Registers.EAX, SourceReg = CPUx86.Registers.ESP, SourceIsIndirect = true, SourceDisplacement = ( ( aSize / 4 ) * 4 ) };
                        new CPUx86.Mov { DestinationReg = CPUx86.Registers.EBX, DestinationIsIndirect = true, DestinationDisplacement = ( ( aSize / 4 ) * 4 ), SourceReg = CPUx86.Registers.AX };
                        break;
                    }
                case 3:
                    {
                        new CPUx86.Mov { DestinationReg = CPUx86.Registers.EAX, SourceReg = CPUx86.Registers.ESP, SourceIsIndirect = true, SourceDisplacement = ( ( aSize / 4 ) * 4 ) };
                        new CPUx86.Mov { DestinationReg = CPUx86.Registers.EBX, DestinationIsIndirect = true, DestinationDisplacement = ( ( aSize / 4 ) * 4 ), SourceReg = CPUx86.Registers.AX };
                        new CPUx86.Mov { DestinationReg = CPUx86.Registers.EAX, SourceReg = CPUx86.Registers.ESP, SourceIsIndirect = true, SourceDisplacement = ( ( ( aSize / 4 ) * 4 ) + 2 ) };
                        new CPUx86.Mov { DestinationReg = CPUx86.Registers.EBX, DestinationIsIndirect = true, DestinationDisplacement = ( ( ( aSize / 4 ) * 4 ) + 2 ), SourceReg = CPUx86.Registers.AL };
                        break;
                    }
                default:
                    throw new Exception( "Error, shouldn't occur" );
            }
            new CPUx86.Add { DestinationReg = CPUx86.Registers.ESP, SourceValue = ( uint )( xStorageSize + 4 ) };
        }

      public override void Execute( MethodInfo aMethod, ILOpCode aOpCode )
        {
            Assemble(Assembler, 4, DebugEnabled);
        }


        // using System;
        // using System.IO;
        // 
        // 
        // using CPU = Cosmos.Assembler.x86;
        // using CPUx86 = Cosmos.Assembler.x86;
        // 
        // namespace Cosmos.IL2CPU.IL.X86 {
        // 	[Cosmos.Assembler.OpCode(OpCodeEnum.Stind_I)]
        // 	public class Stind_I: Op {
        // 		public Stind_I(ILReader aReader, MethodInformation aMethodInfo)
        // 			: base(aReader, aMethodInfo) {
        // 		}
        // 		public static void Assemble(Assembler.Assembler aAssembler, int aSize) {
        // 			new CPU.Comment("address at: [esp + " + aSize + "]");
        // 			int xStorageSize = aSize;
        // 			if (xStorageSize < 4) {
        // 				xStorageSize = 4;
        // 			}
        //             new CPUx86.Move { DestinationReg = CPUx86.Registers.EBX, SourceReg = CPUx86.Registers.ESP, SourceIsIndirect = true, SourceDisplacement = xStorageSize };
        // 			for (int i = 0; i < (aSize / 4); i++) {
        //                 new CPUx86.Move { DestinationReg = CPUx86.Registers.EAX, SourceReg = CPUx86.Registers.ESP, SourceIsIndirect = true, SourceDisplacement = i * 4 };
        //                 new CPUx86.Move { DestinationReg = CPUx86.Registers.EBX, DestinationIsIndirect = true, DestinationDisplacement = i * 4, SourceReg = CPUx86.Registers.EAX };
        // 			}
        // 			switch (aSize % 4) {
        // 				case 0: {
        // 						break;
        // 					}
        // 				case 1: {
        //                         new CPUx86.Move { DestinationReg = CPUx86.Registers.EAX, SourceReg = CPUx86.Registers.ESP, SourceIsIndirect = true, SourceDisplacement = ((aSize / 4) * 4) };
        //                         new CPUx86.Move { DestinationReg = CPUx86.Registers.EBX, DestinationIsIndirect = true, SourceDisplacement = ((aSize / 4) * 4), SourceReg = CPUx86.Registers.AL };
        // 						break;
        // 					}
        // 				case 2: {
        // 						new CPUx86.Move{DestinationReg= CPUx86.Registers.EAX, SourceReg=CPUx86.Registers.ESP, SourceIsIndirect=true, SourceDisplacement=((aSize / 4) * 4)};
        //                         new CPUx86.Move { DestinationReg = CPUx86.Registers.EBX, DestinationIsIndirect = true, DestinationDisplacement = ((aSize / 4) * 4), SourceReg = CPUx86.Registers.AX };
        // 						break;
        // 					}
        // 				case 3: {
        //                         new CPUx86.Move { DestinationReg = CPUx86.Registers.EAX, SourceReg = CPUx86.Registers.ESP, SourceIsIndirect = true, SourceDisplacement = ((aSize / 4) * 4) };
        //                         new CPUx86.Move { DestinationReg = CPUx86.Registers.EBX, DestinationIsIndirect = true, DestinationDisplacement = ((aSize / 4) * 4), SourceReg = CPUx86.Registers.AX };
        //                         new CPUx86.Move { DestinationReg = CPUx86.Registers.EAX, SourceReg = CPUx86.Registers.ESP, SourceIsIndirect = true, SourceDisplacement = (((aSize / 4) * 4) + 2) };
        //                         new CPUx86.Move { DestinationReg = CPUx86.Registers.EBX, DestinationIsIndirect = true, DestinationDisplacement = (((aSize / 4) * 4) + 2), SourceReg = CPUx86.Registers.AL };
        // 						break;
        // 					}
        // 				default:
        // 					throw new Exception("Error, shouldn't occur");
        // 			}
        //             new CPUx86.Add { DestinationReg = CPUx86.Registers.ESP, SourceValue = (uint)(xStorageSize + 4) };
        // 			aAssembler.Stack.Pop();
        // 			aAssembler.Stack.Pop();
        // 		}
        // 
        // 		public override void DoAssemble() {
        // 			Assemble(Assembler, 4);
        // 		}
        // 	}
        // }

    }
}
