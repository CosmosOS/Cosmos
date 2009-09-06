using System;
using CPUx86 = Indy.IL2CPU.Assembler.X86;
using Cosmos.IL2CPU.ILOpCodes;
namespace Cosmos.IL2CPU.X86.IL
{
    [Cosmos.IL2CPU.OpCode( ILOpCode.Code.Ldobj )]
    public class Ldobj : ILOp
    {
        public Ldobj( Cosmos.IL2CPU.Assembler aAsmblr )
            : base( aAsmblr )
        {
        }

        public override void Execute( MethodInfo aMethod, ILOpCode aOpCode )
        {
            var xSize = Assembler.Stack.Pop();
            OpType xType = ( OpType )aOpCode;
            new CPUx86.Pop { DestinationReg = CPUx86.Registers.EAX };
            for( int i = 1; i <= ( xSize.Size / 4 ); i++ )
            {
                new CPUx86.Push { DestinationReg = CPUx86.Registers.EAX, DestinationIsIndirect = true, DestinationDisplacement = ( int )( xSize.Size - ( i * 4 ) ) };
            }

            switch( xSize.Size % 4 )
            {
                case 1:
                    {
                        new CPUx86.Xor { DestinationReg = CPUx86.Registers.EBX, SourceReg = CPUx86.Registers.EBX };
                        new CPUx86.Move { DestinationReg = CPUx86.Registers.BL, SourceIsIndirect = true, SourceReg = CPUx86.Registers.EAX };
                        new CPUx86.Push { DestinationReg = CPUx86.Registers.EBX };
                        break;
                    }
                case 2:
                    {
                        new CPUx86.Xor { DestinationReg = CPUx86.Registers.EBX, SourceReg = CPUx86.Registers.EBX };
                        new CPUx86.Move { DestinationReg = CPUx86.Registers.BX, SourceIsIndirect = true, SourceReg = CPUx86.Registers.EAX };
                        new CPUx86.Push { DestinationReg = CPUx86.Registers.EBX };
                        break;
                    }
                case 0:
                    {
                        break;
                    }
                default:
                        throw new Exception( "Remainder not supported!" );
            }
            Assembler.Stack.Pop();

            //TODO: Push type not number
            Assembler.Stack.Push( new StackContents.Item( ( int )xSize.Size, xType.Value  ) );
        }


        // using System;
        // using Indy.IL2CPU.Assembler.X86;
        // using CPUx86 = Indy.IL2CPU.Assembler.X86;
        // using Indy.IL2CPU.Assembler;
        // using Indy.IL2CPU.Compiler;
        // 
        // namespace Indy.IL2CPU.IL.X86 {
        //     [OpCode(OpCodeEnum.Ldobj)]
        //     public class Ldobj : Op {
        //         private Type xType;
        // 
        //         public Ldobj(ILReader aReader,
        //                      MethodInformation aMethodInfo)
        //             : base(aReader,
        //                    aMethodInfo) {
        //             xType = aReader.OperandValueType;
        //             if (xType == null) {
        //                 throw new Exception("Type specification not found!");
        //             }
        //         }
        // 
        //         public override void DoAssemble() {
        //             var xSize = GetService<IMetaDataInfoService>().SizeOfType(xType);
        //             new CPUx86.Pop { DestinationReg = CPUx86.Registers.EAX };
        //             for (int i = 1; i <= (xSize / 4); i++) {
        //                 new CPUx86.Push { DestinationReg = Registers.EAX, DestinationIsIndirect = true, DestinationDisplacement = (int)(xSize - (i * 4)) };
        //             }
        //             switch (xSize % 4) {
        //                 case 1: {
        //                         new CPUx86.Xor { DestinationReg = CPUx86.Registers.EBX, SourceReg = CPUx86.Registers.EBX };
        //                     new CPUx86.Move { DestinationReg = CPUx86.Registers.BL, SourceIsIndirect = true, SourceReg = CPUx86.Registers.EAX };
        //                     new CPUx86.Push { DestinationReg = Registers.EBX };
        //                     break;
        //                 }
        //                 case 2: {
        //                         new CPUx86.Xor { DestinationReg = CPUx86.Registers.EBX, SourceReg = CPUx86.Registers.EBX };
        //                     new CPUx86.Move { DestinationReg = CPUx86.Registers.BX, SourceIsIndirect = true, SourceReg = CPUx86.Registers.EAX };
        //                     new CPUx86.Push{DestinationReg=Registers.EBX};
        //                     break;
        //                 }
        //                 case 0: {
        //                     break;
        //                 }
        //                 default: {
        //                     throw new Exception("Remainder not supported!");
        //                 }
        //             }
        //             Assembler.Stack.Pop();
        //             Assembler.Stack.Push(new StackContent((int)xSize,
        //                                                           xType));
        //         }
        //     }
        // }

    }
}
