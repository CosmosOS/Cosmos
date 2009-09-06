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
            int xSize = Math.Max( Assembler.Stack.Pop().Size, Assembler.Stack.Pop().Size );

            string BaseLabel = GetLabel( aMethod, aOpCode ) + "__";
            string LabelTrue = BaseLabel + "True";
            string LabelFalse = BaseLabel + "False";

            if( xSize > 8 )
            {
                throw new Exception( "StackSizes>8 not supported" );
            }
            if( xSize <= 4 )
            {
                Assembler.Stack.Push( new StackContents.Item( 4, typeof( bool ) ) );

                new CPUx86.Pop { DestinationReg = CPUx86.Registers.EAX };
                new CPUx86.Compare { DestinationReg = CPUx86.Registers.EAX, SourceReg = CPUx86.Registers.ESP, SourceIsIndirect = true };
                new CPUx86.ConditionalJump { Condition = CPUx86.ConditionalTestEnum.Equal, DestinationLabel = LabelTrue };
                new CPUx86.Jump { DestinationLabel = LabelFalse };
                new Label( LabelTrue );
                new CPUx86.Add { DestinationReg = CPUx86.Registers.ESP, SourceValue = 4 };
                new CPUx86.Push { DestinationValue = 1 };
                //new CPUx86.Jump { DestinationLabel = NextInstructionLabel };
                Jump_End(aMethod);
                new Label( LabelFalse );
                new CPUx86.Add { DestinationReg = CPUx86.Registers.ESP, SourceValue = 4 };
                new CPUx86.Push { DestinationValue = 0 };
                //new CPUx86.Jump { DestinationLabel = NextInstructionLabel };
                Jump_End(aMethod);
                return;
            }
            if( xSize > 4 )
            {
                Assembler.Stack.Push( new StackContents.Item( 4, typeof( bool ) ) );

                new CPUx86.Pop { DestinationReg = CPUx86.Registers.EAX };
                new CPUx86.Compare { DestinationReg = CPUx86.Registers.EAX, SourceReg = CPUx86.Registers.ESP, SourceIsIndirect = true, SourceDisplacement = 4 };

                new CPUx86.Pop { DestinationReg = CPUx86.Registers.EAX };
                new CPUx86.ConditionalJump { Condition = CPUx86.ConditionalTestEnum.NotEqual, DestinationLabel = LabelFalse };

                new CPUx86.Xor { DestinationReg = CPUx86.Registers.EAX, SourceReg = CPUx86.Registers.ESP, SourceIsIndirect = true, SourceDisplacement = 4 };
                new CPUx86.ConditionalJump { Condition = CPUx86.ConditionalTestEnum.NotZero, DestinationLabel = LabelFalse };

                //they are equal, eax == 0
                new CPUx86.Add { DestinationReg = CPUx86.Registers.ESP, SourceValue = 8 };
                new CPUx86.Add { DestinationReg = CPUx86.Registers.EAX, SourceValue = 1 };
                new CPUx86.Push { DestinationReg = CPUx86.Registers.EAX };
                //new CPUx86.Jump { DestinationLabel = NextInstructionLabel };
                Jump_End( aMethod );
                new Label( LabelFalse );
                //eax = 0
                new CPUx86.Add { DestinationReg = CPUx86.Registers.ESP, SourceValue = 8 };
                new CPUx86.Xor { DestinationReg = CPUx86.Registers.EAX, SourceReg = CPUx86.Registers.EAX };
                new CPUx86.Push { DestinationReg = CPUx86.Registers.EAX };
                //new CPUx86.Jump { DestinationLabel = NextInstructionLabel };
                Jump_End( aMethod );
                return;
            }
            throw new Exception( "Case not handled!" );
        }


        // using System;
        // using System.Linq;
        // 
        // using CPU = Cosmos.IL2CPU.X86;
        // using CPUx86 = Cosmos.IL2CPU.X86;
        // using Cosmos.IL2CPU.X86;
        // 
        // namespace Indy.IL2CPU.IL.X86 {
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
