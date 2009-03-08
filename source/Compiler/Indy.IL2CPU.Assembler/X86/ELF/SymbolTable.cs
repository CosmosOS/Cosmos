using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Indy.IL2CPU.Assembler.X86.ELF {
    public class SymbolTable : ELFFileContentBase {
        public SymbolTable(SectionHeader aHeader) {
            mHeader=aHeader;
            Symbols = new List<Symbol>();
        }

        private SectionHeader mHeader;
        public override void ReadFromStream(Stream aInput) {
            while (aInput.Position < (mHeader.Offset + mHeader.Size)) {
                var xSymbol = new Symbol();
                xSymbol.File = File;
                xSymbol.ReadFromStream(aInput);
                Symbols.Add(xSymbol);
            }
        }

        public override void DumpInfo(StringBuilder aOutput,
                                      string aPrefix) {
            for(int i = 0; i < Symbols.Count;i++) {
                Symbols[i].DumpInfo(aOutput, aPrefix + "[" + i + "]\t");
            }
        }

        public IList<Symbol> Symbols {
            get;
            private set;
        }

        public override uint DetermineSize(uint aStartAddress) {
            return (uint)(Symbols.Count * 16);
        }

        public override void WriteToStream(Stream aOutput) {
            foreach (var xSymbol in Symbols) {
                xSymbol.WriteToStream(aOutput);
            }
        }
    }
}