using Cosmos.IL2CPU.Plugs;
using XSharp.Compiler;
using CPUx86 = Cosmos.Assembler.x86;

namespace Cosmos.Core.Plugs.System.Assemblers
{
    public class Buffer_BlockCopy : AssemblerMethod
    {

        /*public static void BlockCopy(
         *			Array src, [ebp + 24]
         *			int srcOffset, [ebp + 20]
         *			Array dst, [ebp + 16]
         *			int dstOffset, [ebp + 12]
         *			int count); [ebp + 8]
         */
        public override void AssembleNew(Assembler.Assembler aAssembler, object aMethodInfo)
        {
            XS.Set(XSRegisters.ESI, XSRegisters.EBP, sourceDisplacement: 24);
            XS.Add(XSRegisters.ESI, 16);
            XS.Set(XSRegisters.EAX, XSRegisters.EBP, sourceDisplacement: 20);
            XS.Add(XSRegisters.ESI, XSRegisters.EAX);

            XS.Set(XSRegisters.EDI, XSRegisters.EBP, sourceDisplacement: 16);
            XS.Add(XSRegisters.EDI, 16);
            XS.Set(XSRegisters.EAX, XSRegisters.EBP, sourceDisplacement: 12);
            XS.Add(XSRegisters.EDI, XSRegisters.EAX);

            XS.Set(XSRegisters.ECX, XSRegisters.EBP, sourceDisplacement: 8);
            new CPUx86.Movs { Size = 8, Prefixes = CPUx86.InstructionPrefixes.Repeat };
        }
    }
}
