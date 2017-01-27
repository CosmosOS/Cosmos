using System;

using XSharp.Compiler;
using static XSharp.Compiler.XSRegisters;

namespace Cosmos.IL2CPU.X86.IL
{
	  /// <summary>
	  /// Convert top Stack element to Int64.
	  /// </summary>
    [Cosmos.IL2CPU.OpCode( ILOpCode.Code.Conv_I8 )]
    public class Conv_I8 : ILOp
    {
        public Conv_I8( Cosmos.Assembler.Assembler aAsmblr )
            : base( aAsmblr )
        {
        }

        public override void Execute( MethodInfo aMethod, ILOpCode aOpCode )
        {
            var xSource = aOpCode.StackPopTypes[0];
            var xSize = SizeOfType(xSource);
            var xIsFloat = TypeIsFloat(xSource);

            if (xSize > 8)
            {
                throw new NotImplementedException("Cosmos.IL2CPU.x86->IL->Conv_I8.cs->Error: StackSize > 8 not supported");
            }

            if (xSize <= 4)
            {
                if (xIsFloat)
                {
                    /*
                     * Sadly for x86 there is no way using SSE to convert a float to an Int64... in x64 we could use ConvertPD2DQAndTruncate with
                     * x64 register as a destination... so this one of the few cases in which we need the legacy FPU!
                     */
                    XS.FPU.FloatLoad(ESP, destinationIsIndirect: true, size: RegisterSize.Int32);
                    XS.Sub(ESP, 4);
                    XS.FPU.IntStoreWithTruncate(ESP, isIndirect: true, size: RegisterSize.Long64);
                }
                else
                {
                    XS.Pop(EAX);
                    XS.SignExtendAX(RegisterSize.Int32);
                    XS.Push(EDX);
                    XS.Push(EAX);
                }
            }
            else if (xSize <= 8)
            {
                if (xIsFloat)
                {
                    /*
                     * Sadly for x86 there is no way using SSE to convert a double to an Int64... in x64 we could use ConvertPD2DQAndTruncate with
                     * x64 register as a destination... so only in this case we need the legacy FPU!
                     */
                    XS.FPU.FloatLoad(ESP, destinationIsIndirect: true, size: RegisterSize.Long64);
                    XS.FPU.IntStoreWithTruncate(ESP, isIndirect: true, size: RegisterSize.Long64);
                }
            }
        }
    }
}
