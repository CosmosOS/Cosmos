using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.Compiler.Assembler
{
    public class DataEndIfDefined: DataMember, IEndIfDefined {
        public DataEndIfDefined()
            : base("define", new byte[0]) {
        }

        public override void WriteText(Assembler aAssembler, System.IO.TextWriter aOutput)
        {
            aOutput.Write(this.GetAsText());
        }
    }
}