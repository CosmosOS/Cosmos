using XSharp.Compiler;

namespace Cosmos.IL2CPU.Plugs.Assemblers.CPU
{
    public class CPUZeroFillAsm : AssemblerMethod
    {
        public override void AssembleNew(Assembler.Assembler aAssembler, object aMethodInfo)
        {
            XS.ClearDirectionFlag();
            XS.Set(XSRegisters.EDI, XSRegisters.EBP, sourceDisplacement: 0xC); //address
            XS.Set(XSRegisters.ECX, XSRegisters.EBP, sourceDisplacement: 0x8); //length
            // set EAX to value of fill (zero)
            XS.Xor(XSRegisters.EAX, XSRegisters.EAX);
            XS.ShiftRight(XSRegisters.ECX, 1);
            XS.Jump(Assembler.x86.ConditionalTestEnum.NotBelow, ".step2");
            XS.StoreByteInString();
            XS.Label(".step2");
            XS.ShiftRight(XSRegisters.ECX, 1);
            XS.Jump(Assembler.x86.ConditionalTestEnum.NotBelow, ".step3");
            XS.StoreWordInString();
            XS.Label(".step3");
            new Assembler.x86.Stos {Size = 32, Prefixes = Assembler.x86.InstructionPrefixes.Repeat};
        }
    }
}
