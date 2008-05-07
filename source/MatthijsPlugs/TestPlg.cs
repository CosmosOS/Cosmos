using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Indy.IL2CPU.Assembler;
using Indy.IL2CPU.Assembler.X86;
using Indy.IL2CPU.Plugs;

namespace MatthijsPlugs {
    [Plug(Target=typeof(MatthijsTest.Program))]
    public class TestPlug {
        [PlugMethod(MethodAssembler = typeof(GetResumeAndResumeAssembler))]
        public static void GetResumeAndResume(out uint aSuspend,
                                              out uint aResume) {
            aSuspend = 0;
            aResume = 0;
        }
    }

    internal class GetResumeAndResumeAssembler: AssemblerMethod {
        public override void Assemble(Indy.IL2CPU.Assembler.Assembler aAssembler) {
            new Move("eax",
                     "[ebp - 4]");
            new Move("dword",
                     "[eax]",
                     "[DebugResumeLevel]");
            new Move("eax",
                     "[ebp - 8]");
            new Move("dword",
                     "[eax]",
                     "[DebugSuspendLevel]");
            throw new NotImplementedException();
        }
    }
}