using System;
using Cosmos.Assembler;
using Cosmos.IL2CPU.ILOpCodes;
using CPUx86 = Cosmos.Assembler.x86;
using CPU = Cosmos.Assembler.x86;
using Cosmos.Assembler.x86;
namespace Cosmos.IL2CPU.X86.IL
{
    /// <summary>
    /// Compares two values. If the first value is less than the second, the integer value 1 (int32) is pushed onto the evaluation stack;
    /// otherwise 0 (int32) is pushed onto the evaluation stack.
    /// </summary>
    [Cosmos.IL2CPU.OpCode( ILOpCode.Code.Clt )]
    public class Clt : ILOp
    {
        public Clt( Cosmos.Assembler.Assembler aAsmblr )
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
                //EmitNotImplementedException( Assembler, GetServiceProvider(), "Clt: StackSizes>8 not supported", CurInstructionLabel, mMethodInfo, mCurrentOffset, NextInstructionLabel );
                throw new NotImplementedException("Cosmos.IL2CPU.x86->IL->Clt.cs->Error: StackSizes > 8 not supported");
                //return;
            }
            string BaseLabel = GetLabel( aMethod, aOpCode ) + ".";
            string LabelTrue = BaseLabel + "True";
            string LabelFalse = BaseLabel + "False";
            if( xStackItemSize > 4 )
            {
				new CPUx86.Mov { DestinationReg = CPUx86.RegistersEnum.ESI, SourceValue = 1 };
				// esi = 1
				new CPUx86.Xor { DestinationReg = CPUx86.RegistersEnum.EDI, SourceReg = CPUx86.RegistersEnum.EDI };
				// edi = 0
				if (xStackItemIsFloat)
				{
					// value 2
					new CPUx86.x87.FloatLoad { DestinationReg = RegistersEnum.ESP, Size = 64, DestinationIsIndirect = true };
					// value 1
					new CPUx86.x87.FloatLoad { DestinationReg = RegistersEnum.ESP, Size = 64, DestinationDisplacement = 8, DestinationIsIndirect = true };
					new CPUx86.x87.FloatCompareAndSet { DestinationReg = RegistersEnum.ST1 };
					// if carry is set, ST(0) < ST(i)
					new CPUx86.ConditionalMove { Condition = CPUx86.ConditionalTestEnum.Below, DestinationReg = CPUx86.RegistersEnum.EDI, SourceReg = CPUx86.RegistersEnum.ESI };
					// pops fpu stack
					new CPUx86.x87.FloatStoreAndPop { DestinationReg = CPUx86.RegistersEnum.ST0 };
					new CPUx86.x87.FloatStoreAndPop { DestinationReg = CPUx86.RegistersEnum.ST0 };
					new CPUx86.Add { DestinationReg = RegistersEnum.ESP, SourceValue = 16 };
                }
                else
                {
                    new CPUx86.Pop { DestinationReg = CPUx86.RegistersEnum.EAX };
                    new CPUx86.Pop { DestinationReg = CPUx86.RegistersEnum.EDX };
                    //value2: EDX:EAX
                    new CPUx86.Pop { DestinationReg = CPUx86.RegistersEnum.EBX };
                    new CPUx86.Pop { DestinationReg = CPUx86.RegistersEnum.ECX };
                    //value1: ECX:EBX
                    new CPUx86.Sub { DestinationReg = CPUx86.RegistersEnum.EBX, SourceReg = CPUx86.RegistersEnum.EAX };
                    new CPUx86.SubWithCarry { DestinationReg = CPUx86.RegistersEnum.ECX, SourceReg = CPUx86.RegistersEnum.EDX };
                    //result = value1 - value2
					new CPUx86.ConditionalMove { Condition = CPUx86.ConditionalTestEnum.LessThan, DestinationReg = CPUx86.RegistersEnum.EDI, SourceReg = CPUx86.RegistersEnum.ESI };
                }
                new CPUx86.Push { DestinationReg = CPUx86.RegistersEnum.EDI };
            }
            else
            {
                if (xStackItemIsFloat)
                {
                    new CPUx86.SSE.MoveSS { DestinationReg = CPUx86.RegistersEnum.XMM0, SourceReg = CPUx86.RegistersEnum.ESP, SourceIsIndirect = true };
                    new CPUx86.Add { DestinationReg = CPUx86.RegistersEnum.ESP, SourceValue = 4 };
                    new CPUx86.SSE.MoveSS { DestinationReg = CPUx86.RegistersEnum.XMM1, SourceReg = CPUx86.RegistersEnum.ESP, SourceIsIndirect = true };
                    new CPUx86.SSE.CompareSS { DestinationReg = CPUx86.RegistersEnum.XMM1, SourceReg = CPUx86.RegistersEnum.XMM0, pseudoOpcode = (byte)CPUx86.SSE.ComparePseudoOpcodes.LessThan };
                    new CPUx86.MoveD { DestinationReg = CPUx86.RegistersEnum.EBX, SourceReg = CPUx86.RegistersEnum.XMM1 };
                    new CPUx86.And { DestinationReg = CPUx86.RegistersEnum.EBX, SourceValue = 1 };
                    new CPUx86.Mov { SourceReg = CPUx86.RegistersEnum.EBX, DestinationReg = CPUx86.RegistersEnum.ESP, DestinationIsIndirect = true };
                }
                else
                {
                    new CPUx86.Pop { DestinationReg = CPUx86.RegistersEnum.ECX };
                    new CPUx86.Pop { DestinationReg = CPUx86.RegistersEnum.EAX };
                    new CPUx86.Push { DestinationReg = CPUx86.RegistersEnum.ECX };
                    new CPUx86.Compare { DestinationReg = CPUx86.RegistersEnum.EAX, SourceReg = CPUx86.RegistersEnum.ESP, SourceIsIndirect = true };
                    new CPUx86.ConditionalJump { Condition = CPUx86.ConditionalTestEnum.LessThan, DestinationLabel = LabelTrue };
                    new CPUx86.Jump { DestinationLabel = LabelFalse };
                    new Label( LabelTrue );
                    new CPUx86.Add { DestinationReg = CPUx86.RegistersEnum.ESP, SourceValue = 4 };
                    new CPUx86.Push { DestinationValue = 1 };

                    new CPUx86.Jump { DestinationLabel = GetLabel(aMethod, aOpCode.NextPosition) };
                
                    new Label( LabelFalse );
                    new CPUx86.Add { DestinationReg = CPUx86.RegistersEnum.ESP, SourceValue = 4 };
                    new CPUx86.Push { DestinationValue = 0 };
                }
            }
        }


        // using System;
        // using System.IO;
        // 
        // 
        // using CPUx86 = Cosmos.Assembler.x86;
        // using CPU = Cosmos.Assembler.x86;
        // using Cosmos.IL2CPU.X86;
        // using Cosmos.IL2CPU.X86;
        // 
        // namespace Cosmos.IL2CPU.IL.X86 {
        // 	[Cosmos.Assembler.OpCode(OpCodeEnum.Clt)]
        // 	public class Clt: Op {
        // 		private readonly string NextInstructionLabel;
        // 		private readonly string CurInstructionLabel;
        //         private uint mCurrentOffset;
        //         private MethodInformation mMethodInfo;
        //         public Clt(ILReader aReader, MethodInformation aMethodInfo)
        // 			: base(aReader, aMethodInfo) {
        // 			NextInstructionLabel = GetInstructionLabel(aReader.NextPosition);
        // 			CurInstructionLabel = GetInstructionLabel(aReader);
        //             mMethodInfo = aMethodInfo;
        //             mCurrentOffset = aReader.Position;
        // 		}
        // 		public override void DoAssemble() {

        // 		}
        // 	}
        // }

    }
}
