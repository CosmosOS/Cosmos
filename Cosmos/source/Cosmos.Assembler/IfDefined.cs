using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.Assembler
{
    [Cosmos.Assembler.OpCode("%ifdef")]
    public class IfDefined: Instruction, IIfDefined {
        public string Symbol {
            get;
            set;
        }

        public IfDefined(string aSymbol) {
            Symbol = aSymbol;
        }

        public override void WriteText(Cosmos.Assembler.Assembler aAssembler, System.IO.TextWriter aOutput)
        {
            aOutput.Write(this.GetAsText());
        }
    }
}