using System;
using CPUx86 = Indy.IL2CPU.Assembler.X86;
using CPU = Indy.IL2CPU.Assembler;
namespace Cosmos.IL2CPU.X86.IL
{
    [Cosmos.IL2CPU.OpCode( ILOpCode.Code.Clt_Un )]
    public class Clt_Un : ILOp
    {
        public Clt_Un( Cosmos.IL2CPU.Assembler aAsmblr )
            : base( aAsmblr )
        {
        }

        public override void Execute( MethodInfo aMethod, ILOpCode aOpCode )
        {
            var xStackItem = Assembler.Stack.Pop();
            if( xStackItem.IsFloat )
            {
                //EmitNotImplementedException( Assembler, GetServiceProvider(), "Clt_Un: Floats not yet supported", CurInstructionLabel, mMethodInfo, mCurrentOffset, NextInstructionLabel );
                throw new NotImplementedException(); 
                return;
            }
            if( xStackItem.Size > 8 )
            {
                //EmitNotImplementedException( Assembler, GetServiceProvider(), "Clt_Un: StackSizes>8 not supported", CurInstructionLabel, mMethodInfo, mCurrentOffset, NextInstructionLabel );
                throw new NotImplementedException();
                return;
            }
            Assembler.Stack.Push( new StackContents.Item( 4, typeof( bool ) ) );
            string BaseLabel = GetLabel( aMethod, aOpCode ) + "__";
            string LabelTrue = BaseLabel + "True";
            string LabelFalse = BaseLabel + "False";
            if( xStackItem.Size > 4 )
            {
                new CPUx86.Xor { DestinationReg = CPUx86.Registers.ESI, SourceReg = CPUx86.Registers.ESI };
                new CPUx86.Add { DestinationReg = CPUx86.Registers.ESI, SourceValue = 1 };
                new CPUx86.Xor { DestinationReg = CPUx86.Registers.EDI, SourceReg = CPUx86.Registers.EDI };
                //esi = 1
                new CPUx86.Pop { DestinationReg = CPUx86.Registers.EAX };
                new CPUx86.Pop { DestinationReg = CPUx86.Registers.EDX };
                //value2: EDX:EAX
                new CPUx86.Pop { DestinationReg = CPUx86.Registers.EBX };
                new CPUx86.Pop { DestinationReg = CPUx86.Registers.ECX };
                //value1: ECX:EBX
                new CPUx86.Sub { DestinationReg = CPUx86.Registers.EBX, SourceReg = CPUx86.Registers.EAX };
                new CPUx86.SubWithCarry { DestinationReg = CPUx86.Registers.ECX, SourceReg = CPUx86.Registers.EDX };
                //result = value1 - value2
                new CPUx86.ConditionalMove { Condition = CPUx86.ConditionalTestEnum.Below, DestinationReg = CPUx86.Registers.EDI, SourceReg = CPUx86.Registers.ESI };
                new CPUx86.Push { DestinationReg = CPUx86.Registers.EDI };
            }
            else
            {
                new CPUx86.Pop { DestinationReg = CPUx86.Registers.ECX };
                new CPUx86.Pop { DestinationReg = CPUx86.Registers.EAX };
                new CPUx86.Push { DestinationReg = CPUx86.Registers.ECX };
                new CPUx86.Compare { DestinationReg = CPUx86.Registers.EAX, SourceReg = CPUx86.Registers.ESP, SourceIsIndirect = true };
                new CPUx86.ConditionalJump { Condition = CPUx86.ConditionalTestEnum.LessThan, DestinationLabel = LabelTrue };
                new CPUx86.Jump { DestinationLabel = LabelFalse };
                new CPU.Label( LabelTrue );
                new CPUx86.Add { DestinationReg = CPUx86.Registers.ESP, SourceValue = 4 };
                new CPUx86.Push { DestinationValue = 1 };
                //new CPUx86.Jump { DestinationLabel = NextInstructionLabel };
                Jump_End( aMethod );
                new CPU.Label( LabelFalse );
                new CPUx86.Add { DestinationReg = CPUx86.Registers.ESP, SourceValue = 4 };
                new CPUx86.Push { DestinationValue = 0 };
                //new CPUx86.Jump { DestinationLabel = NextInstructionLabel };
                Jump_End( aMethod );
            }
        }


        // using System;
        // using System.IO;
        // 
        // 
        // using CPUx86 = Indy.IL2CPU.Assembler.X86;
        // using CPU = Indy.IL2CPU.Assembler;
        // using Indy.IL2CPU.Assembler;
        // using Indy.IL2CPU.Assembler.X86;
        // 
        // namespace Indy.IL2CPU.IL.X86 {
        // 	[OpCode(OpCodeEnum.Clt_Un)]
        // 	public class Clt_Un: Op {
        // 		private readonly string NextInstructionLabel;
        // 		private readonly string CurInstructionLabel;
        //         private uint mCurrentOffset;
        //         private MethodInformation mMethodInfo;
        //         public Clt_Un(ILReader aReader, MethodInformation aMethodInfo)
        // 			: base(aReader, aMethodInfo) {
        // 			NextInstructionLabel = GetInstructionLabel(aReader.NextPosition);
        // 			CurInstructionLabel = GetInstructionLabel(aReader);
        //             mMethodInfo = aMethodInfo;
        //             mCurrentOffset = aReader.Position;
        // 		}
        // 		public override void DoAssemble() {
        // 			var xStackItem= Assembler.Stack.Pop();
        // 			if (xStackItem.IsFloat) {
        //                 EmitNotImplementedException(Assembler, GetServiceProvider(), "Clt_Un: Floats not yet supported", CurInstructionLabel, mMethodInfo, mCurrentOffset, NextInstructionLabel);
        // 			    return;
        // 			}
        // 			if (xStackItem.Size > 8) {
        //                 EmitNotImplementedException(Assembler, GetServiceProvider(), "Clt_Un: StackSizes>8 not supported", CurInstructionLabel, mMethodInfo, mCurrentOffset, NextInstructionLabel);
        // 			    return;
        // 			}
        // 			Assembler.Stack.Push(new StackContent(4, typeof(bool)));
        // 			string BaseLabel = CurInstructionLabel + "__";
        // 			string LabelTrue = BaseLabel + "True";
        // 			string LabelFalse = BaseLabel + "False";
        //             if (xStackItem.Size > 4)
        // 			{
        //                 new CPUx86.Xor { DestinationReg = CPUx86.Registers.ESI, SourceReg = CPUx86.Registers.ESI };
        //                 new CPUx86.Add { DestinationReg = Registers.ESI, SourceValue = 1 };
        //                 new CPUx86.Xor { DestinationReg = CPUx86.Registers.EDI, SourceReg = CPUx86.Registers.EDI };
        // 				//esi = 1
        // 				new CPUx86.Pop{DestinationReg = CPUx86.Registers.EAX};
        //                 new CPUx86.Pop { DestinationReg = CPUx86.Registers.EDX };
        // 				//value2: EDX:EAX
        //                 new CPUx86.Pop { DestinationReg = CPUx86.Registers.EBX };
        //                 new CPUx86.Pop { DestinationReg = CPUx86.Registers.ECX };
        // 				//value1: ECX:EBX
        //                 new CPUx86.Sub { DestinationReg = CPUx86.Registers.EBX, SourceReg = CPUx86.Registers.EAX };
        //                 new CPUx86.SubWithCarry { DestinationReg = CPUx86.Registers.ECX, SourceReg = CPUx86.Registers.EDX };
        // 				//result = value1 - value2
        //                 new CPUx86.ConditionalMove{ Condition=ConditionalTestEnum.Below, DestinationReg = CPUx86.Registers.EDI, SourceReg = CPUx86.Registers.ESI };
        // 				new CPUx86.Push { DestinationReg = Registers.EDI };
        // 			} else
        // 			{
        //                 new CPUx86.Pop { DestinationReg = CPUx86.Registers.ECX };
        //                 new CPUx86.Pop { DestinationReg = CPUx86.Registers.EAX };
        //                 new CPUx86.Push { DestinationReg = CPUx86.Registers.ECX };
        //                 new CPUx86.Compare { DestinationReg = CPUx86.Registers.EAX, SourceReg = CPUx86.Registers.ESP, SourceIsIndirect = true };
        //                 new CPUx86.ConditionalJump { Condition = CPUx86.ConditionalTestEnum.LessThan, DestinationLabel = LabelTrue };
        //                 new CPUx86.Jump { DestinationLabel = LabelFalse };
        // 				new CPU.Label(LabelTrue);
        //                 new CPUx86.Add { DestinationReg = CPUx86.Registers.ESP, SourceValue = 4 };
        //                 new CPUx86.Push { DestinationValue = 1 };
        //                 new CPUx86.Jump { DestinationLabel = NextInstructionLabel };
        // 				new CPU.Label(LabelFalse);
        //                 new CPUx86.Add { DestinationReg = CPUx86.Registers.ESP, SourceValue = 4 };
        //                 new CPUx86.Push { DestinationValue = 0 };
        //                 new CPUx86.Jump { DestinationLabel = NextInstructionLabel };
        // 			}
        // 		}
        // 	}
        // }

    }
}
