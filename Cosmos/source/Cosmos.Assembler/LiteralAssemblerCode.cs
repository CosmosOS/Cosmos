using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.Assembler
{
    public class LiteralAssemblerCode: Instruction
    {
        public LiteralAssemblerCode(string code)
        {
            Code = code;
        }

        public string Code
        {
            get;
            set;
        }

        public override void WriteText(Assembler aAssembler, System.IO.TextWriter aOutput)
        {
            aOutput.Write(Code);
        }
    }
}
