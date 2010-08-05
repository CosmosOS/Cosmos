using System;
using CPUx86 = Cosmos.Compiler.Assembler.X86;
using Cosmos.Compiler.Assembler;
using Cosmos.IL2CPU.IL.CustomImplementations.System;
namespace Cosmos.IL2CPU.X86.IL
{
    [Cosmos.IL2CPU.OpCode( ILOpCode.Code.Ldelem_Ref )]
    public class Ldelem_Ref : ILOp
    {
        public Ldelem_Ref( Cosmos.Compiler.Assembler.Assembler aAsmblr )
            : base( aAsmblr )
        {
        }
        public static void Assemble( Assembler aAssembler, uint aElementSize )
        {
            
            new CPUx86.Pop { DestinationReg = CPUx86.Registers.EAX };
            new CPUx86.Move { DestinationReg = CPUx86.Registers.EDX, SourceValue = aElementSize };
            new CPUx86.Multiply { DestinationReg = CPUx86.Registers.EDX };

            //TODO: implement ObjectImpl first
            new CPUx86.Add { DestinationReg = CPUx86.Registers.EAX, SourceValue = ( ObjectImpl.FieldDataOffset + 4 ) };

            new CPUx86.Pop { DestinationReg = CPUx86.Registers.EDX };
            new CPUx86.Add { DestinationReg = CPUx86.Registers.EDX, SourceReg = CPUx86.Registers.EAX };
            new CPUx86.Move { DestinationReg = CPUx86.Registers.EAX, SourceReg = CPUx86.Registers.EDX };
            uint xSizeLeft = aElementSize;
            while( xSizeLeft > 0 )
            {
                if( xSizeLeft >= 4 )
                {
                    new CPUx86.Push { DestinationReg = CPUx86.Registers.EAX, DestinationIsIndirect = true };
                    new CPUx86.Add { DestinationReg = CPUx86.Registers.EAX, SourceValue = 4 };
                    xSizeLeft -= 4;
                }
                else
                {
                    if( xSizeLeft >= 2 )
                    {
                        new CPUx86.Move { DestinationReg = CPUx86.Registers.ECX, SourceValue = 0 };
                        new CPUx86.Move { DestinationReg = CPUx86.Registers.CX, SourceReg = CPUx86.Registers.EAX, SourceIsIndirect = true };
                        new CPUx86.Push { DestinationReg = CPUx86.Registers.ECX };
                        new CPUx86.Add { DestinationReg = CPUx86.Registers.EAX, SourceValue = 2 };
                        xSizeLeft -= 2;
                    }
                    else
                    {
                        if( xSizeLeft >= 1 )
                        {
                            new CPUx86.Move { DestinationReg = CPUx86.Registers.ECX, SourceValue = 0 };
                            new CPUx86.Move { DestinationReg = CPUx86.Registers.CL, SourceReg = CPUx86.Registers.EAX, SourceIsIndirect = true };
                            new CPUx86.Push { DestinationReg = CPUx86.Registers.ECX };
                            new CPUx86.Add { DestinationReg = CPUx86.Registers.EAX, SourceValue = 1 };
                            xSizeLeft -= 1;
                        }
                        else
                        {
                            throw new Exception( "Size left: " + xSizeLeft );
                        }
                    }
                }
            }
            aAssembler.Stack.Pop();
            aAssembler.Stack.Pop();
            aAssembler.Stack.Push(  ( int )aElementSize, true, false, false ) ;
        }
        public override void Execute( MethodInfo aMethod, ILOpCode aOpCode )
        {
            Assemble( Assembler, 4 );
        }


        // using System;
        // using System.IO;
        // using CPU = Cosmos.Compiler.Assembler.X86;
        // using CPUx86 = Cosmos.Compiler.Assembler.X86;
        // using Cosmos.IL2CPU.X86;
        // 
        // namespace Cosmos.IL2CPU.IL.X86 {
        // 	[OpCode(OpCodeEnum.Ldelem_Ref)]
        // 	public class Ldelem_Ref: Op {
        // 		public Ldelem_Ref(ILReader aReader, MethodInformation aMethodInfo)
        // 			: base(aReader, aMethodInfo) {
        // 		}
        // 
        // 		public static void Assemble(CPU.Assembler aAssembler, uint aElementSize) {
        // 			new CPUx86.Pop{DestinationReg=CPUx86.Registers.EAX};
        //             new CPUx86.Move { DestinationReg = CPUx86.Registers.EDX, SourceValue = aElementSize };
        //             new CPUx86.Multiply { DestinationReg = CPUx86.Registers.EDX };
        //             new CPUx86.Add { DestinationReg = CPUx86.Registers.EAX, SourceValue = (ObjectImpl.FieldDataOffset + 4) };
        //             new CPUx86.Pop { DestinationReg = CPUx86.Registers.EDX };
        //             new CPUx86.Add { DestinationReg = CPUx86.Registers.EDX, SourceReg = CPUx86.Registers.EAX };
        //             new CPUx86.Move { DestinationReg = CPUx86.Registers.EAX, SourceReg = CPUx86.Registers.EDX };
        // 			uint xSizeLeft = aElementSize;
        // 			while (xSizeLeft > 0) {
        // 				if (xSizeLeft >= 4) {
        //                     new CPUx86.Push { DestinationReg = CPUx86.Registers.EAX, DestinationIsIndirect=true};
        //                     new CPUx86.Add { DestinationReg = CPUx86.Registers.EAX, SourceValue = 4 };
        // 					xSizeLeft -= 4;
        // 				} else {
        // 					if (xSizeLeft >= 2) {
        //                         new CPUx86.Move { DestinationReg = CPUx86.Registers.ECX, SourceValue = 0 };
        //                         new CPUx86.Move { DestinationReg = CPUx86.Registers.CX, SourceReg = CPUx86.Registers.EAX, SourceIsIndirect = true };
        //                         new CPUx86.Push { DestinationReg = CPUx86.Registers.ECX };
        //                         new CPUx86.Add { DestinationReg = CPUx86.Registers.EAX, SourceValue = 2 };
        // 						xSizeLeft -= 2;
        // 					} else {
        // 						if (xSizeLeft >= 1) {
        //                             new CPUx86.Move { DestinationReg = CPUx86.Registers.ECX, SourceValue = 0 };
        //                             new CPUx86.Move { DestinationReg = CPUx86.Registers.CL, SourceReg = CPUx86.Registers.EAX, SourceIsIndirect = true };
        //                             new CPUx86.Push { DestinationReg = CPUx86.Registers.ECX };
        //                             new CPUx86.Add { DestinationReg = CPUx86.Registers.EAX, SourceValue = 1 };
        // 							xSizeLeft -= 1;
        // 						} else {
        // 							throw new Exception("Size left: " + xSizeLeft);
        // 						}
        // 					}
        // 				}
        // 			}
        // 			aAssembler.Stack.Pop();
        // 			aAssembler.Stack.Pop();
        // 			aAssembler.Stack.Push(new StackContent((int)aElementSize, true, false, false));
        // 		}
        // 
        // 		public override void DoAssemble() {
        // 			Assemble(Assembler, 4);
        // 		}
        // 	}
        // }

    }
}
