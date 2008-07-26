using System;
using System.Linq;
using Indy.IL2CPU.Assembler;
using Indy.IL2CPU.Plugs;
using CPU = Indy.IL2CPU.Assembler;
using CPUx86 = Indy.IL2CPU.Assembler.X86;

namespace Cosmos.Kernel.Plugs.Assemblers {
    public class Interrupt30 : AssemblerMethod {
        /*
         * Arguments with their addresses:
         *   ref uint aEAX  : ebp - 20
         *   ref uint aEBX  : ebp - 16
         *   ref uint aECX  : ebp - 12
         *   ref uint aEDX  : ebp - 8
         */

        public override void Assemble(Assembler aAssembler) {
            new CPUx86.Move("edx",
                            "[ebp + 8]");
            new CPUx86.Move("edx",
                            "[edx]");
            new CPUx86.Move("ecx",
                            "[ebp + 12]");
            new CPUx86.Move("ecx",
                            "[ecx]");
            new CPUx86.Move("ebx",
                            "[ebp + 16]");
            new CPUx86.Move("ebx",
                            "[ebx]");
            new CPUx86.Move("eax",
                            "[ebp + 20]");
            new CPUx86.Move("eax",
                            "[eax]");
            new CPUx86.Interrupt("0x30");
            new CPUx86.Move("edi",
                            "[ebp + 8]");
            new CPUx86.Move("[edi]",
                            "edx");
            new CPUx86.Move("edi",
                            "[ebp + 12]");
            new CPUx86.Move("[edi]",
                            "ecx");
            new CPUx86.Move("edi",
                            "[ebp + 16]");
            new CPUx86.Move("[edi]",
                            "ebx");
            new CPUx86.Move("edi",
                            "[ebp + 20]");
            new CPUx86.Move("[edi]",
                            "eax");
            new CPUx86.Move("ecx",
                            "0");
        }
    }
}