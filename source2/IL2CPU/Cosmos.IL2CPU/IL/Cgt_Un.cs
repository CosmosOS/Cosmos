using System;
using CPUx86 = Cosmos.Assembler.x86;
using CPU = Cosmos.Assembler.x86;
using Cosmos.Assembler;
using Cosmos.Assembler.x86;
namespace Cosmos.IL2CPU.X86.IL
{
    [Cosmos.IL2CPU.OpCode( ILOpCode.Code.Cgt_Un )]
    public class Cgt_Un : ILOp
    {
        public Cgt_Un( Cosmos.Assembler.Assembler aAsmblr )
            : base( aAsmblr )
        {
        }

        public override void Execute( MethodInfo aMethod, ILOpCode aOpCode )
        {
            var xStackItem = aOpCode.StackPopTypes[0];
            var xStackItemSize = SizeOfType(xStackItem);
            var xStackItemIsFloat = TypeIsFloat(xStackItem);
            if( xStackItemSize > 8 )
            {
                //EmitNotImplementedException( Assembler, GetServiceProvider(), "Cgt_Un: StackSizes>8 not supported", CurInstructionLabel, mMethodInfo, mCurrentOffset, NextInstructionLabel );
                throw new NotImplementedException("Cosmos.IL2CPU.x86->IL->Cgt_Un.cs->Error: StackSizes > 8 not supported");
            }
            string BaseLabel = GetLabel( aMethod, aOpCode ) + ".";
            string LabelTrue = BaseLabel + "True";
            string LabelFalse = BaseLabel + "False";
            if( xStackItemSize > 4 )
            {
				new CPUx86.Mov { DestinationReg = CPUx86.Registers.ESI, SourceValue = 1 };
				// esi = 1
				new CPUx86.Xor { DestinationReg = CPUx86.Registers.EDI, SourceReg = CPUx86.Registers.EDI };
				// edi = 0
				if (xStackItemIsFloat)
				{
					// value 1
					new CPUx86.x87.FloatLoad { DestinationReg = Registers.ESP, Size = 64, DestinationDisplacement = 8, DestinationIsIndirect = true };
					// value 2
					new CPUx86.x87.FloatLoad { DestinationReg = Registers.ESP, Size = 64, DestinationIsIndirect = true };
					new CPUx86.x87.FloatCompareAndSet { DestinationReg = Registers.ST1 };
					// if carry is set, ST(0) < ST(i)
					new CPUx86.ConditionalMove { Condition = CPUx86.ConditionalTestEnum.Below, DestinationReg = CPUx86.Registers.EDI, SourceReg = CPUx86.Registers.ESI };
					// pops fpu stack
					new CPUx86.x87.FloatStoreAndPop { DestinationReg = CPUx86.Registers.ST0 };
					new CPUx86.x87.FloatStoreAndPop { DestinationReg = CPUx86.Registers.ST0 };
					new CPUx86.Add { DestinationReg = Registers.ESP, SourceValue = 16 };
				}
                else
                {
                    new CPUx86.Pop { DestinationReg = CPUx86.Registers.EAX };
                    new CPUx86.Pop { DestinationReg = CPUx86.Registers.EDX };
                    //value2: EDX:EAX
                    new CPUx86.Pop { DestinationReg = CPUx86.Registers.EBX };
                    new CPUx86.Pop { DestinationReg = CPUx86.Registers.ECX };
                    //value1: ECX:EBX

					new CPUx86.Compare { DestinationReg = CPUx86.Registers.ECX, SourceReg = CPUx86.Registers.EDX };
					new CPUx86.ConditionalJump { Condition = CPUx86.ConditionalTestEnum.Above, DestinationLabel = LabelTrue };
					new CPUx86.ConditionalJump { Condition = CPUx86.ConditionalTestEnum.Below, DestinationLabel = LabelFalse };
					new CPUx86.Compare { DestinationReg = CPUx86.Registers.EBX, SourceReg = CPUx86.Registers.EAX };
					new Label(LabelTrue);
					new CPUx86.ConditionalMove { Condition = CPUx86.ConditionalTestEnum.Above, DestinationReg = CPUx86.Registers.EDI, SourceReg = CPUx86.Registers.ESI };
					new Label(LabelFalse);
                }
				new CPUx86.Push { DestinationReg = CPUx86.Registers.EDI };
				/*
                new CPUx86.ConditionalJump { Condition = CPUx86.ConditionalTestEnum.Above, DestinationLabel = LabelTrue };
				new Label(LabelFalse);
                new CPUx86.Push { DestinationValue = 0 };
                new CPUx86.Jump { DestinationLabel = GetLabel(aMethod, aOpCode.NextPosition) };
                new Label( LabelTrue );
                new CPUx86.Push { DestinationValue = 1 };
				*/
            }
            else
            {
                if (xStackItemIsFloat)
                {

                    new CPUx86.SSE.MoveSS { DestinationReg = CPUx86.Registers.XMM0, SourceReg = CPUx86.Registers.ESP, SourceIsIndirect = true };
                    new CPUx86.Add { DestinationReg = CPUx86.Registers.ESP, SourceValue = 4 };
                    new CPUx86.SSE.MoveSS { DestinationReg = CPUx86.Registers.XMM1, SourceReg = CPUx86.Registers.ESP, SourceIsIndirect = true };
                    new CPUx86.SSE.CompareSS { DestinationReg = CPUx86.Registers.XMM1, SourceReg = CPUx86.Registers.XMM0, pseudoOpcode = (byte)CPUx86.SSE.ComparePseudoOpcodes.NotLessThanOrEqualTo };
                    new CPUx86.MoveD { DestinationReg = CPUx86.Registers.EBX, SourceReg = CPUx86.Registers.XMM1 };
                    new CPUx86.And { DestinationReg = CPUx86.Registers.EBX, SourceValue = 1 };
                    new CPUx86.Mov { SourceReg = CPUx86.Registers.EBX, DestinationReg = CPUx86.Registers.ESP, DestinationIsIndirect = true };
                }
                else
                {
                    new CPUx86.Pop { DestinationReg = CPUx86.Registers.EAX };
                    new CPUx86.Compare { DestinationReg = CPUx86.Registers.EAX, SourceReg = CPUx86.Registers.ESP, SourceIsIndirect = true };
                
                    new CPUx86.ConditionalJump { Condition = CPUx86.ConditionalTestEnum.Below, DestinationLabel = LabelTrue };
                    new CPUx86.Jump { DestinationLabel = LabelFalse };
                    new Label( LabelTrue );
                    new CPUx86.Add { DestinationReg = CPUx86.Registers.ESP, SourceValue = 4 };
                    new CPUx86.Push { DestinationValue = 1 };
                    new CPUx86.Jump { DestinationLabel = GetLabel(aMethod, aOpCode.NextPosition) };
                    new Label( LabelFalse );
                    new CPUx86.Add { DestinationReg = CPUx86.Registers.ESP, SourceValue = 4 };
                    new CPUx86.Push { DestinationValue = 0 };
                }
            }
        }


        // using System;
        // 
        // using CPUx86 = Cosmos.Assembler.x86;
        // using CPU = Cosmos.Assembler.x86;
        // using Cosmos.IL2CPU.X86;
        // using Cosmos.IL2CPU.X86;
        // 
        // namespace Cosmos.IL2CPU.IL.X86 {
        // 	[Cosmos.Assembler.OpCode(OpCodeEnum.Cgt_Un)]
        // 	public class Cgt_Un: Op {
        // 		private readonly string NextInstructionLabel;
        //         private readonly string CurInstructionLabel;
        //         private uint mCurrentOffset;
        //         private MethodInformation mMethodInfo;
        //         public Cgt_Un(ILReader aReader, MethodInformation aMethodInfo)
        //             : base(aReader, aMethodInfo)
        //         {
        //             NextInstructionLabel = GetInstructionLabel(aReader.NextPosition);
        //             CurInstructionLabel = GetInstructionLabel(aReader);
        //             mMethodInfo = aMethodInfo;
        //             mCurrentOffset = aReader.Position;
        //         }
        // 
        // 	    public override void DoAssemble()
        // 		{
        // 			var xStackItem = Assembler.Stack.Pop();
        //             if (xStackItem.IsFloat)
        //             {
        //                 EmitNotImplementedException(Assembler, GetServiceProvider(), "Cgt_Un: Floats not yet supported", CurInstructionLabel, mMethodInfo, mCurrentOffset, NextInstructionLabel);
        //                 return;
        //             }
        //             if (xStackItem.Size > 8)
        //             {
        //                 EmitNotImplementedException(Assembler, GetServiceProvider(), "Cgt_Un: StackSizes>8 not supported", CurInstructionLabel, mMethodInfo, mCurrentOffset, NextInstructionLabel);
        //                 return;
        //             }
        // 			Assembler.Stack.Push(new StackContent(4, typeof(bool)));
        // 			string BaseLabel = CurInstructionLabel + ".";
        // 			string LabelTrue = BaseLabel + "True";
        // 			string LabelFalse = BaseLabel + "False";
        // 			if (xStackItem.Size > 4)
        // 			{
        //                 new CPUx86.Xor { DestinationReg = CPUx86.Registers.ESI, SourceReg = CPUx86.Registers.ESI};
        //                 new CPUx86.Add { DestinationReg = CPUx86.Registers.ESI, SourceValue = 1 };
        //                 new CPUx86.Xor { DestinationReg = CPUx86.Registers.EDI, SourceReg = CPUx86.Registers.EDI};
        // 				//esi = 1
        //                 new CPUx86.Pop { DestinationReg = CPUx86.Registers.EAX };
        //                 new CPUx86.Pop { DestinationReg = CPUx86.Registers.EDX };
        //                 //value2: EDX:EAX
        //                 new CPUx86.Pop { DestinationReg = CPUx86.Registers.EBX };
        //                 new CPUx86.Pop { DestinationReg = CPUx86.Registers.ECX };
        //                 //value1: ECX:EBX
        //                 new CPUx86.Sub { DestinationReg = CPUx86.Registers.EBX, SourceReg = CPUx86.Registers.EAX };
        //                 new CPUx86.SubWithCarry { DestinationReg = CPUx86.Registers.ECX, SourceReg = CPUx86.Registers.EDX };
        // 				//result = value1 - value2
        // 				//new CPUx86.ConditionalMove(Condition.Above, "edi", "esi");
        //                 //new CPUx86.Push { DestinationReg = Registers.EDI };
        // 
        //                 new CPUx86.ConditionalJump { Condition = CPUx86.ConditionalTestEnum.Above, DestinationLabel = LabelTrue };
        //                 new CPUx86.Push { DestinationValue = 0 };
        //                 new CPUx86.Jump { DestinationLabel = NextInstructionLabel };
        // 
        // 				new Label(LabelTrue);
        // 				new CPUx86.Push{DestinationValue=1};
        // 
        // 			} else
        // 			{
        //                 new CPUx86.Pop{DestinationReg=CPUx86.Registers.EAX};
        //                 new CPUx86.Compare { DestinationReg = CPUx86.Registers.EAX, SourceReg = CPUx86.Registers.ESP, SourceIsIndirect = true };
        //                 new CPUx86.ConditionalJump { Condition = CPUx86.ConditionalTestEnum.Below, DestinationLabel = LabelTrue };
        //                 new CPUx86.Jump { DestinationLabel = LabelFalse };
        //                 new Label(LabelTrue);
        //                 new CPUx86.Add { DestinationReg = CPUx86.Registers.ESP, SourceValue = 4 };
        //                 new CPUx86.Push { DestinationValue = 1 };
        //                 new CPUx86.Jump { DestinationLabel = NextInstructionLabel };
        //                 new Label(LabelFalse);
        //                 new CPUx86.Add { DestinationReg = CPUx86.Registers.ESP, SourceValue = 4 };
        //                 new CPUx86.Push { DestinationValue = 0 };
        //                 new CPUx86.Jump { DestinationLabel = NextInstructionLabel };
        // 			}
        // 		}
        // 	}
        // }

    }
}
