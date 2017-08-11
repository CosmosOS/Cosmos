using XSharp.Assembler;
using XSharp;
using x86 = XSharp.Assembler.x86;

namespace Cosmos.Core_Asm
{
    public class CPUZeroFillAsm : AssemblerMethod
    {
        public override void AssembleNew(Assembler aAssembler, object aMethodInfo)
        {
            XS.ClearDirectionFlag();
            XS.Set(XSRegisters.EDI, XSRegisters.EBP, sourceDisplacement: 0xC); //address
            XS.Set(XSRegisters.ECX, XSRegisters.EBP, sourceDisplacement: 0x8); //length
            // set EAX to value of fill (zero)
            XS.Xor(XSRegisters.EAX, XSRegisters.EAX);
            XS.ShiftRight(XSRegisters.ECX, 1);
            XS.Jump(x86.ConditionalTestEnum.NotBelow, ".step2");
            XS.StoreByteInString();
            XS.Label(".step2");
            XS.ShiftRight(XSRegisters.ECX, 1);
            XS.Jump(x86.ConditionalTestEnum.NotBelow, ".step3");
            XS.StoreWordInString();
            XS.Label(".step3");
            new x86.Stos { Size = 32, Prefixes = x86.InstructionPrefixes.Repeat };
        }
    }
}
