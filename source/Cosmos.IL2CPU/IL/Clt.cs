using System;
using Cosmos.Assembler;
using Cosmos.IL2CPU.ILOpCodes;
using CPUx86 = Cosmos.Assembler.x86;
using CPU = Cosmos.Assembler.x86;
using Cosmos.Assembler.x86;
using Cosmos.Assembler.x86.SSE;
using Cosmos.Assembler.x86.x87;
using XSharp.Compiler;
using static XSharp.Compiler.XSRegisters;
using static Cosmos.Assembler.x86.SSE.ComparePseudoOpcodes;

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
                // Using SSE registers (that do NOT branch!) This is needed only for long now
#if false
				XS.Set(XSRegisters.ESI, 1);
				// esi = 1
				XS.Xor(XSRegisters.EDI, XSRegisters.EDI);
				// edi = 0
#endif
                if (xStackItemIsFloat)
                {
                    // Please note that SSE supports double operations only from version 2
                    XS.SSE2.MoveSD(XMM0, ESP, sourceIsIndirect: true);
                    // Increment ESP to get the value of the next double
                    XS.Add(ESP, 8);
                    XS.SSE2.MoveSD(XMM1, ESP, sourceIsIndirect: true);
                    XS.SSE2.CompareSD(XMM1, XMM0, comparision: LessThan);
                    XS.SSE2.MoveD(EBX, XMM1);
                    XS.And(EBX, 1);
                    // We need to move the stack pointer of 4 Byte to "eat" the second double that is yet in the stack or we get a corrupted stack!
                    XS.Add(ESP, 4);
                    XS.Set(ESP, EBX, destinationIsIndirect: true);
                }
                else
                {
                    XS.Set(XSRegisters.ESI, 1);
                    // esi = 1
                    XS.Xor(XSRegisters.EDI, XSRegisters.EDI);
                    // edi = 0
                    XS.Pop(XSRegisters.EAX);
                    XS.Pop(XSRegisters.EDX);
                    //value2: EDX:EAX
                    XS.Pop(XSRegisters.EBX);
                    XS.Pop(XSRegisters.ECX);
                    //value1: ECX:EBX
                    XS.Sub(XSRegisters.EBX, XSRegisters.EAX);
                    XS.SubWithCarry(XSRegisters.ECX, XSRegisters.EDX);
                    //result = value1 - value2
                    
                    new CPUx86.ConditionalMove { Condition = CPUx86.ConditionalTestEnum.LessThan, DestinationReg = XSRegisters.EDI, SourceReg = XSRegisters.ESI };
                    XS.Push(XSRegisters.EDI);
                }
            }
            else
            {
                if (xStackItemIsFloat)
                {
                    XS.SSE.MoveSS(XMM0, ESP, sourceIsIndirect: true);
                    XS.Add(XSRegisters.ESP, 4);
                    XS.SSE.MoveSS(XMM1, ESP, sourceIsIndirect: true);
                    XS.SSE.CompareSS(XMM1, XMM0, comparision: LessThan);
                    XS.SSE2.MoveD(EBX, XMM1);
                    XS.And(XSRegisters.EBX, 1);
                    XS.Set(ESP, EBX, destinationIsIndirect: true);
                }
                else
                {
                    XS.Pop(XSRegisters.ECX);
                    XS.Pop(XSRegisters.EAX);
                    XS.Push(XSRegisters.ECX);
                    XS.Compare(EAX, ESP, sourceIsIndirect: true);
                    XS.Jump(ConditionalTestEnum.LessThan, LabelTrue);
                    XS.Jump(LabelFalse);
                    XS.Label(LabelTrue );
                    XS.Add(XSRegisters.ESP, 4);
                    XS.Push(1);

                    new Jump { DestinationLabel = GetLabel(aMethod, aOpCode.NextPosition) };

                    XS.Label(LabelFalse );
                    XS.Add(XSRegisters.ESP, 4);
                    XS.Push(0);
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
