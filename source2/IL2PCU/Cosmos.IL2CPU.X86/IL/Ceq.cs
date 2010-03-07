using System;
using CPUx86 = Cosmos.IL2CPU.X86;
using CPU = Cosmos.IL2CPU.X86;
namespace Cosmos.IL2CPU.X86.IL
{
    [Cosmos.IL2CPU.OpCode( ILOpCode.Code.Ceq )]
    public class Ceq : ILOp
    {
        public Ceq( Cosmos.IL2CPU.Assembler aAsmblr )
            : base( aAsmblr )
        {
        }

        public override void Execute( MethodInfo aMethod, ILOpCode aOpCode )
        {
            var xStackItem=Assembler.Stack.Pop();
            var xStackItem2=Assembler.Stack.Pop();
            int xSize = Math.Max(xStackItem.Size, xStackItem2.Size);

            string BaseLabel = GetLabel( aMethod, aOpCode ) + "__";
            string LabelTrue = BaseLabel + "True";
            string LabelFalse = BaseLabel + "False";
            var xNextLabel = GetLabel(aMethod, aOpCode.NextPosition);

            if( xSize > 8 )
            {
                throw new Exception( "StackSizes>8 not supported" );
            }
            if( xSize <= 4 )
            {
                Assembler.Stack.Push( new StackContents.Item( 4, typeof( bool ) ) );
                if (xStackItem.IsFloat)
                {
                    new CPUx86.SSE.MoveSS { DestinationReg = CPUx86.Registers.XMM0, SourceReg = CPUx86.Registers.ESP, SourceIsIndirect = true };
                    new CPUx86.Add { DestinationReg = CPUx86.Registers.ESP, SourceValue = 4 };
                    new CPUx86.SSE.MoveSS { DestinationReg = CPUx86.Registers.XMM1, SourceReg = CPUx86.Registers.ESP, SourceIsIndirect = true };
                    new CPUx86.SSE.CompareSS { DestinationReg = CPUx86.Registers.XMM1, SourceReg = CPUx86.Registers.XMM0, pseudoOpcode = (byte)CPUx86.SSE.ComparePseudoOpcodes.Equal };
                    new CPUx86.MoveD {DestinationReg = CPUx86.Registers.EBX, SourceReg = CPUx86.Registers.XMM1};
                    new CPUx86.And { DestinationReg = CPUx86.Registers.EBX, SourceValue = 1};
                    new CPUx86.Move { SourceReg = CPUx86.Registers.EBX, DestinationReg = CPUx86.Registers.ESP, DestinationIsIndirect = true };
                }
                else
                {
                    new CPUx86.Pop { DestinationReg = CPUx86.Registers.EAX };
                    new CPUx86.Compare { DestinationReg = CPUx86.Registers.EAX, SourceReg = CPUx86.Registers.ESP, SourceIsIndirect = true };
                    new CPUx86.ConditionalJump { Condition = CPUx86.ConditionalTestEnum.Equal, DestinationLabel = LabelTrue };
                    new CPUx86.Jump { DestinationLabel = LabelFalse };
                    new Label(LabelTrue);
                    new CPUx86.Add { DestinationReg = CPUx86.Registers.ESP, SourceValue = 4 };
                    new CPUx86.Push { DestinationValue = 1 };
                    new CPUx86.Jump { DestinationLabel = xNextLabel };
                    new Label(LabelFalse);
                    new CPUx86.Add { DestinationReg = CPUx86.Registers.ESP, SourceValue = 4 };
                    new CPUx86.Push { DestinationValue = 0 };
                    new CPUx86.Jump { DestinationLabel = xNextLabel };
                }
            }
            else if( xSize > 4 )
            {
                Assembler.Stack.Push( new StackContents.Item( 4, typeof( bool ) ) );

                if (xStackItem.IsFloat)
                {
                    new CPUx86.x87.FloatLoad { DestinationReg = Registers.ESP, Size = 64, DestinationIsIndirect = true };
                    new CPUx86.Add { SourceValue = 8, DestinationReg = Registers.ESP };
                    new CPUx86.x87.FloatCompare { DestinationReg = Registers.ESP, DestinationIsIndirect = true };
                    new CPUx86.Add { SourceValue = 8, DestinationReg = Registers.ESP };
                    new CPUx86.x87.FloatDecTopPointer();
                    throw new NotImplementedException();
                }
                else
                {
                    new CPUx86.Pop { DestinationReg = CPUx86.Registers.EAX };
                    new CPUx86.Compare { DestinationReg = CPUx86.Registers.EAX, SourceReg = CPUx86.Registers.ESP, SourceIsIndirect = true, SourceDisplacement = 4 };
                    new CPUx86.Pop { DestinationReg = CPUx86.Registers.EAX };

                    new CPUx86.ConditionalJump { Condition = CPUx86.ConditionalTestEnum.NotEqual, DestinationLabel = LabelFalse };
                    new CPUx86.Compare { DestinationReg = CPUx86.Registers.EAX, SourceReg = CPUx86.Registers.ESP, SourceIsIndirect = true, SourceDisplacement = 4 };

                    new CPUx86.ConditionalJump { Condition = CPUx86.ConditionalTestEnum.NotZero, DestinationLabel = LabelFalse };

                    //they are equal, eax == 0
                    new CPUx86.Add { DestinationReg = CPUx86.Registers.ESP, SourceValue = 8 };
                    //new CPUx86.Add { DestinationReg = CPUx86.Registers.EAX, SourceValue = 1 };
                    new CPUx86.Push { DestinationValue =1 };
                    new CPUx86.Jump { DestinationLabel = xNextLabel };
                    new Label(LabelFalse);
                    //eax = 0
                    new CPUx86.Add { DestinationReg = CPUx86.Registers.ESP, SourceValue = 8 };
                    //new CPUx86.Xor { DestinationReg = CPUx86.Registers.EAX, SourceReg = CPUx86.Registers.EAX };
                    new CPUx86.Push { DestinationValue = 0 };
                    new CPUx86.Jump { DestinationLabel = xNextLabel };
                }
            }
            else
                throw new Exception( "Case not handled!" );
        }


        // using System;
        // using System.Linq;
        // 
        // using CPU = Cosmos.IL2CPU.X86;
        // using CPUx86 = Cosmos.IL2CPU.X86;
        // using Cosmos.IL2CPU.X86;
        // 
        // namespace Cosmos.IL2CPU.IL.X86 {
        // 	[OpCode(OpCodeEnum.Ceq)]
        // 	public class Ceq: Op {
        // 		private readonly string NextInstructionLabel;
        // 		private readonly string CurInstructionLabel;
        // 		public Ceq(ILReader aReader, MethodInformation aMethodInfo)
        // 			: base(aReader, aMethodInfo) {
        // 			NextInstructionLabel = GetInstructionLabel(aReader.NextPosition);
        // 			CurInstructionLabel = GetInstructionLabel(aReader);
        // 		}
        // 		private void Assemble4Byte() {
        // 			Assembler.Stack.Push(new StackContent(4, typeof(bool)));
        // 			string BaseLabel = CurInstructionLabel + "__";
        // 			string LabelTrue = BaseLabel + "True";
        // 			string LabelFalse = BaseLabel + "False";
        //             new CPUx86.Pop { DestinationReg = CPUx86.Registers.EAX };
        //             new CPUx86.Compare { DestinationReg = CPUx86.Registers.EAX, SourceReg = CPUx86.Registers.ESP, SourceIsIndirect = true };
        //             new CPUx86.ConditionalJump { Condition = CPUx86.ConditionalTestEnum.Equal, DestinationLabel = LabelTrue };
        //             new CPUx86.Jump { DestinationLabel = LabelFalse };
        // 			new Label(LabelTrue);
        // 			new CPUx86.Add{DestinationReg = CPUx86.Registers.ESP, SourceValue=4};
        //             new CPUx86.Push { DestinationValue = 1 };
        //             new CPUx86.Jump { DestinationLabel = NextInstructionLabel };
        // 			new Label(LabelFalse);
        //             new CPUx86.Add { DestinationReg = CPUx86.Registers.ESP, SourceValue = 4 };
        // 			new CPUx86.Push{DestinationValue=0};
        //             new CPUx86.Jump { DestinationLabel = NextInstructionLabel };
        // 		}
        // 
        // 		private void Assemble8Byte() {
        // 			Assembler.Stack.Push(new StackContent(4, typeof(bool)));
        // 			string BaseLabel = CurInstructionLabel + "__";
        // 			string LabelTrue = BaseLabel + "True";
        // 			string LabelFalse = BaseLabel + "False";
        // 
        //             new CPUx86.Pop { DestinationReg = CPUx86.Registers.EAX };
        //             new CPUx86.Compare { DestinationReg = CPUx86.Registers.EAX, SourceReg = CPUx86.Registers.ESP, SourceIsIndirect = true, SourceDisplacement = 4 };
        // 
        //             new CPUx86.Pop { DestinationReg = CPUx86.Registers.EAX };
        //             new CPUx86.ConditionalJump { Condition = CPUx86.ConditionalTestEnum.NotEqual, DestinationLabel = LabelFalse };
        // 
        //             new CPUx86.Xor { DestinationReg = CPUx86.Registers.EAX, SourceReg = CPUx86.Registers.ESP, SourceIsIndirect = true, SourceDisplacement = 4 };
        //             new CPUx86.ConditionalJump { Condition = CPUx86.ConditionalTestEnum.NotZero, DestinationLabel = LabelFalse };
        // 
        // 			//they are equal, eax == 0
        //             new CPUx86.Add { DestinationReg = CPUx86.Registers.ESP, SourceValue = 8 };
        //             new CPUx86.Add { DestinationReg = CPUx86.Registers.EAX, SourceValue = 1 };
        //             new CPUx86.Push { DestinationReg = CPUx86.Registers.EAX };
        //             new CPUx86.Jump { DestinationLabel = NextInstructionLabel };
        // 
        // 			new Label(LabelFalse);
        // 			//eax = 0
        //             new CPUx86.Add { DestinationReg = CPUx86.Registers.ESP, SourceValue = 8 };
        //             new CPUx86.Xor { DestinationReg = CPUx86.Registers.EAX, SourceReg = CPUx86.Registers.EAX };
        //             new CPUx86.Push { DestinationReg = CPUx86.Registers.EAX };
        //             new CPUx86.Jump { DestinationLabel = NextInstructionLabel };
        // 		}
        // 
        // 		public override void DoAssemble() {
        // 			int xSize = Math.Max(Assembler.Stack.Pop().Size, Assembler.Stack.Pop().Size);
        // 			if (xSize > 8) {
        // 				throw new Exception("StackSizes>8 not supported");
        // 			}
        // 			if (xSize <= 4) {
        // 				Assemble4Byte();
        // 				return;
        // 			}
        // 			if (xSize > 4) {
        // 				Assemble8Byte();
        // 				return;
        // 			}
        // 			throw new Exception("Case not handled!");
        // 		}
        // 	}
        // }

    }
}
