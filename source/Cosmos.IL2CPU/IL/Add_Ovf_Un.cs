using System;
using CPUx86 = Cosmos.Assembler.x86;
using Cosmos.Assembler.x86;
using Cosmos.Assembler;
using Cosmos.Assembler.x86.SSE;
using XSharp.Compiler;
using static XSharp.Compiler.XSRegisters;

/* Add.Ovf is unsigned integer addition with check for overflow */
namespace Cosmos.IL2CPU.X86.IL
{
    [Cosmos.IL2CPU.OpCode( ILOpCode.Code.Add_Ovf_Un )]
    public class Add_Ovf_Un : ILOp
    {
		public Add_Ovf_Un(Cosmos.Assembler.Assembler aAsmblr)
            : base( aAsmblr )
        {
        }

        public override void Execute( MethodInfo aMethod, ILOpCode aOpCode )
        {
			// TODO overflow check for float
            var xType = aOpCode.StackPopTypes[0];
            var xSize = SizeOfType(xType);
            var xIsFloat = TypeIsFloat(xType);

            if (xIsFloat)
            {
                throw new Exception("Cosmos.IL2CPU.x86->IL->Add_Ovf_Un.cs->Error: Expected unsigned integer operands but get float!");
            }

            if (xSize > 8)
            {
                //EmitNotImplementedException( Assembler, aServiceProvider, "Size '" + xSize.Size + "' not supported (add)", aCurrentLabel, aCurrentMethodInfo, aCurrentOffset, aNextLabel );
                throw new NotImplementedException("Cosmos.IL2CPU.x86->IL->Add_Ovf_Un.cs->Error: StackSize > 8 not supported");
            }
            else
            {
				var xBaseLabel = GetLabel(aMethod, aOpCode) + ".";
				var xSuccessLabel = xBaseLabel + "Success";
                if (xSize > 4) // long
                {
                    XS.Pop(XSRegisters.EDX); // low part
                    XS.Pop(XSRegisters.EAX); // high part
                    XS.Add(ESP, EDX, destinationIsIndirect: true);
					XS.AddWithCarry(ESP, EAX, destinationDisplacement: 4);
                }
                else //integer
                {
                    XS.Pop(XSRegisters.EAX);
                    XS.Add(ESP, EAX, destinationIsIndirect: true);
                }

                // Let's check if we add overflow and if so throw OverflowException
				XS.Jump(ConditionalTestEnum.NotCarry, xSuccessLabel);
                ThrowOverflowException();
			    XS.Label(xSuccessLabel);
            }
        }
    }
}
