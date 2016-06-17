using System;
using CPUx86 = Cosmos.Assembler.x86;
using CPU = Cosmos.Assembler.x86;
using Cosmos.Assembler;
using XSharp.Compiler;
using static XSharp.Compiler.XSRegisters;

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
                XS.Pop(OldToNewRegister(CPUx86.RegistersEnum.EAX)); // shift amount
                XS.Pop(OldToNewRegister(CPUx86.RegistersEnum.EBX)); // value
                XS.Set(OldToNewRegister(CPUx86.RegistersEnum.CL), OldToNewRegister(CPUx86.RegistersEnum.AL));
                XS.ShiftRight(OldToNewRegister(CPUx86.RegistersEnum.EBX), CL);
                XS.Push(OldToNewRegister(CPUx86.RegistersEnum.EBX));
                return;
            }
            if( xStackItem_Value_Size <= 8 )
            {
                XS.Pop(OldToNewRegister(CPUx86.RegistersEnum.EDX));
                XS.Set(OldToNewRegister(CPUx86.RegistersEnum.EAX), 0);
                XS.Label(xBaseLabel + "__StartLoop" );
                XS.Compare(OldToNewRegister(CPUx86.RegistersEnum.EDX), OldToNewRegister(CPUx86.RegistersEnum.EAX));
                XS.Jump(CPUx86.ConditionalTestEnum.Equal, xBaseLabel + "__EndLoop");
                XS.Set(EBX, ESP, sourceIsIndirect: true);
                XS.Set(OldToNewRegister(CPUx86.RegistersEnum.CL), 1);
                XS.ShiftRight(OldToNewRegister(CPUx86.RegistersEnum.EBX), CL);
                XS.Set(ESP, EBX, destinationIsIndirect: true);
                XS.Set(OldToNewRegister(CPUx86.RegistersEnum.CL), 1);
                XS.RotateThroughCarryRight(ESP, CL, destinationDisplacement: 4, size: RegisterSize.Int32);
                XS.Add(OldToNewRegister(CPUx86.RegistersEnum.EAX), 1);
                new CPUx86.Jump { DestinationLabel = xBaseLabel + "__StartLoop" };

                XS.Label(xBaseLabel + "__EndLoop" );
                return;
            }
        }

    }
}
