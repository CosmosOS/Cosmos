using System;

using XSharp.Compiler;
using static XSharp.Compiler.XSRegisters;

namespace Cosmos.IL2CPU.X86.IL
{
	/// <summary>
	/// Convert top Stack element to Int32.
    /// </summary>
    [Cosmos.IL2CPU.OpCode(ILOpCode.Code.Conv_I)] // x86 is a 32-bit system, so this is the op-code that we should be using, for an x64 target, use Conv_I8 instead.
    [Cosmos.IL2CPU.OpCode( ILOpCode.Code.Conv_I4 )]
    public class Conv_I4 : ILOp
    {
        public Conv_I4( Cosmos.Assembler.Assembler aAsmblr )
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
                throw new NotImplementedException("Cosmos.IL2CPU.x86->IL->Conv_I4.cs->Error: StackSize > 8 not supported");
            }

            if (xSize <= 4)
            {
                if (xIsFloat)
                {
                    XS.SSE.ConvertSS2SIAndTruncateIndirectSource(EAX, ESP);
                    XS.Set(ESP, EAX, destinationIsIndirect: true);
                }
            }
            else if (xSize <= 8)
            {
                if (TypeIsReferenceType(xSource))
                {
                    XS.Add(ESP, 4);
                }
                else
                {
                    if (xIsFloat)
                    {
                        XS.SSE2.ConvertSD2SIAndTruncateIndirectSource(EAX, ESP);
                        XS.Add(ESP, 4);
                        XS.Set(ESP, EAX, destinationIsIndirect: true);
                    }
                    else
                    {
                        XS.Pop(EAX);
                        XS.Add(ESP, 4);
                        XS.Push(EAX);
                    }
                }
            }
        }
    }
}
