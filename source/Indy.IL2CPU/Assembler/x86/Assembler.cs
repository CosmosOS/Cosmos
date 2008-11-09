using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Indy.IL2CPU.Assembler.X86 {
	public class Assembler : Indy.IL2CPU.Assembler.Assembler {
        public override void FlushText(TextWriter aOutput) {
            aOutput.WriteLine("use32");
            aOutput.WriteLine("org 0x200000");
            base.FlushText(aOutput);
        }
	}
}
