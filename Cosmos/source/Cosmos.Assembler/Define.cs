using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.Assembler {
    [Cosmos.Assembler.OpCode("%define")]
    public class Define: Instruction, IDefine {
        public string Symbol {
            get;
            set;
        }

        public Define(string aSymbol) {
            Symbol = aSymbol;
        }

        public override void WriteText(Cosmos.Assembler.Assembler aAssembler, System.IO.TextWriter aOutput)
        {
            aOutput.Write(this.GetAsText());
        }
    }
}