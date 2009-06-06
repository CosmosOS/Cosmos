using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Indy.IL2CPU.Assembler {
    public class DataDefine: DataMember, IDefine {
        public DataDefine(string aSymbol)
            : base("define", new byte[0]) {
            Symbol = aSymbol;
        }

        public string Symbol {
            get;
            set;
        }

        public override void WriteText(Assembler aAssembler, System.IO.TextWriter aOutput)
        {
            aOutput.Write(this.GetAsText());
        }
    }
}