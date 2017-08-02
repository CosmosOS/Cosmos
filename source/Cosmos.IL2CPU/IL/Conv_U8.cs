using System;

using XSharp.Common;
using static XSharp.Common.XSRegisters;

namespace Cosmos.IL2CPU.X86.IL
{
    /// <summary>
    /// Convert top Stack element to UInt64 and change its type to Int64.
    /// </summary>
    [OpCode(ILOpCode.Code.Conv_U8)]
    public class Conv_U8 : ILOp
    {
        public Conv_U8(Cosmos.Assembler.Assembler aAsmblr)
            : base(aAsmblr)
        {
        }

        public override void Execute(_MethodInfo aMethod, ILOpCode aOpCode)
        {
            var xSource = aOpCode.StackPopTypes[0];
            var xSourceSize = SizeOfType(xSource);
            var xSourceIsFloat = TypeIsFloat(xSource);

            if (xSourceSize > 8)
            {
                throw new NotImplementedException("Cosmos.IL2CPU.x86->IL->Conv_U8.cs->Error: StackSize > 8 not supported");
            }

            if (xSourceSize <= 4)
            {
                if (xSourceIsFloat)
                {
                    XS.FPU.FloatLoad(ESP, destinationIsIndirect: true, size: RegisterSize.Int32);
                    XS.Sub(ESP, 4);
                    XS.FPU.IntStoreWithTruncate(ESP, isIndirect: true, size: RegisterSize.Long64);
                }
                else
                {
                    XS.Pop(EAX);
                    XS.Push(0);
                    XS.Push(EAX);
                }
            }
            else if (xSourceSize <= 8)
            {
                if (xSourceIsFloat)
                {
                    XS.FPU.FloatLoad(ESP, destinationIsIndirect: true, size: RegisterSize.Long64);
                    /* The sign of the value should not be changed a negative value is simply converted to its corresponding ulong value */
                    //XS.FPU.FloatAbs();
                    XS.FPU.IntStoreWithTruncate(ESP, isIndirect: true, size: RegisterSize.Long64);
                }
            }
        }
    }
}
