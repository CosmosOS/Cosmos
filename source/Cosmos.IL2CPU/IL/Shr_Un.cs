using System;
using CPUx86 = Cosmos.Assembler.x86;
using CPU = Cosmos.Assembler.x86;
using Cosmos.Assembler;
using XSharp.Compiler;

namespace Cosmos.IL2CPU.X86.IL
{
    [Cosmos.IL2CPU.OpCode( ILOpCode.Code.Shr_Un )]
    public class Shr_Un : ILOp
    {
        public Shr_Un( Cosmos.Assembler.Assembler aAsmblr )
            : base( aAsmblr )
        {
        }

        public override void Execute( MethodInfo aMethod, ILOpCode aOpCode )
        {
            string xBaseLabel = GetLabel( aMethod, aOpCode ) + ".";
            var xStackItem_ShiftAmount = aOpCode.StackPopTypes[0];
            var xStackItem_Value = aOpCode.StackPopTypes[1];
            if (TypeIsFloat(xStackItem_Value))
            {
                throw new NotImplementedException("Floats not yet supported!");
            }
            var xStackItem_Value_Size = SizeOfType(xStackItem_Value);
            if( xStackItem_Value_Size <= 4 )
            {
                XS.Pop(XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.EAX)); // shift amount
                XS.Pop(XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.EBX)); // value
                XS.Set(XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.CL), XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.AL));
                XS.ShiftRight(XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.EBX), XSRegisters.CL);
                XS.Push(XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.EBX));
                return;
            }
            if( xStackItem_Value_Size <= 8 )
            {
                XS.Pop(XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.EDX));
                XS.Set(XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.EAX), 0);
                XS.Label(xBaseLabel + "__StartLoop" );
                XS.Compare(XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.EDX), XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.EAX));
                new CPUx86.ConditionalJump { Condition = CPUx86.ConditionalTestEnum.Equal, DestinationLabel = xBaseLabel + "__EndLoop" };
                XS.Set(XSRegisters.EBX, XSRegisters.ESP, sourceIsIndirect: true);
                XS.Set(XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.CL), 1);
                XS.ShiftRight(XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.EBX), XSRegisters.CL);
                new CPUx86.Mov { DestinationReg = CPUx86.RegistersEnum.ESP, DestinationIsIndirect = true, SourceReg = CPUx86.RegistersEnum.EBX };
                XS.Set(XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.CL), 1);
                new CPUx86.RotateThroughCarryRight { DestinationReg = CPUx86.RegistersEnum.ESP, DestinationIsIndirect = true, DestinationDisplacement = 4, Size = 32, SourceReg = CPUx86.RegistersEnum.CL };
                XS.Add(XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.EAX), 1);
                new CPUx86.Jump { DestinationLabel = xBaseLabel + "__StartLoop" };

                XS.Label(xBaseLabel + "__EndLoop" );
                return;
            }
        }

    }
}
