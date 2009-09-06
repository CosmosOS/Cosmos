using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Cosmos.IL2CPU.X86.ELF {
    public class File : BaseDataStructure {
        public File() {
            Header = new Header() { File = this };
            SectionHeaders = new List<SectionHeader>();
            SectionHeaders.Add(new SectionHeader() { File = this });
            StringTables = new List<StringTable>();
            var xStrTableHeader = new SectionHeader();
            xStrTableHeader.Name = 1;
            SectionHeaders.Add(xStrTableHeader);
            var xStrTbl = new StringTable(xStrTableHeader);
            xStrTableHeader.File = this;
            xStrTableHeader.ActualContent = xStrTbl;
            StringTables.Add(xStrTbl);
            xStrTableHeader.Name = xStrTbl.GetStringIndex(".shstrtab");
            SymbolTables = new List<SymbolTable>();
            ProgramHeaders = new List<ProgramHeaderEntry>();
            BinarySections = new List<BinarySection>();
        }
        public Header Header {
            get;
            private set;
        }
        public override void ReadFromStream(Stream aInput) {
            Header.ReadFromStream(aInput);
            if (Header.ShOffset != 0) {
                ReadSectionHeaders(aInput);
            }
            StringTables.Clear();
            foreach (var xSectionHeader in SectionHeaders) {
                switch (xSectionHeader.Type) {
                    case SectionHeader.SHT_STRTAB:
                        // string table
                        var xStrTable = new StringTable(xSectionHeader);
                        aInput.Position = xSectionHeader.Offset;
                        xStrTable.ReadFromStream(aInput);
                        StringTables.Add(xStrTable);
                        break;
                    case SectionHeader.SHT_SYMTAB:
                        var xSymTable = new SymbolTable(xSectionHeader);
                        aInput.Position = xSectionHeader.Offset;
                        xSymTable.File = this;
                        xSymTable.ReadFromStream(aInput);
                        SymbolTables.Add(xSymTable);
                        break;
                }
            }
        }

        public override uint DetermineSize(uint aStartAddress) {
            throw new NotImplementedException();
        }

        public IList<SectionHeader> SectionHeaders {
            get;
            private set;
        }

        private void ReadSectionHeaders(Stream aInput) {
            SectionHeaders.Clear();
            for (int i = 0; i < Header.ShNum; i++) {
                var xSH = new SectionHeader();
                aInput.Position = Header.ShOffset + (Header.ShEntSize * i);
                xSH.File = this;
                xSH.ReadFromStream(aInput);
                SectionHeaders.Add(xSH);
            }
        }

        public override void DumpInfo(StringBuilder aOutput, string aPrefix) {
            aOutput.AppendLine("Header:");
            Header.DumpInfo(aOutput, aPrefix + "\t");
            aOutput.AppendLine("Section Headers:");
            for (int i = 0; i < SectionHeaders.Count; i++) {
                SectionHeaders[i].DumpInfo(aOutput, "[" + i + "]\t" + aPrefix);
            }
            aOutput.AppendLine("String Tables:");
            for (int i = 0; i < StringTables.Count; i++) {
                StringTables[i].DumpInfo(aOutput, "[" + i + "]\t" + aPrefix);
            }
            aOutput.AppendLine("Symbol Tables:");
            for (int i = 0; i < SymbolTables.Count; i++) {
                SymbolTables[i].DumpInfo(aOutput, "[" + i + "]\t" + aPrefix);
            }

        }

        public List<StringTable> StringTables {
            get;
            private set;
        }

        public List<SymbolTable> SymbolTables {
            get;
            private set;
        }

        public List<BinarySection> BinarySections {
            get;
            private set;
        }

        public List<ProgramHeaderEntry> ProgramHeaders {
            get;
            private set;
        }

        public override void WriteToStream(Stream aOutput) {
            var xCodeItems = new List<BaseDataStructure>();
            xCodeItems.Add(Header);
            foreach (var xPh in ProgramHeaders) {
                xCodeItems.Add(xPh);
            }
            Header.ShStrNdx = 1;
            foreach (var xStrSection in StringTables) {
                xCodeItems.Add(xStrSection);
            }
            foreach (var xSymSection in SymbolTables) {
                xCodeItems.Add(xSymSection);
            }
            foreach (var xBinSection in BinarySections) {
                xCodeItems.Add(xBinSection);
            }
            foreach (var xSectionHeader in SectionHeaders) {
                xCodeItems.Add(xSectionHeader);
            }

            uint xCurPosition = 0;
            for (int i = 0; i < xCodeItems.Count; i++) {
                xCodeItems[i].FileOffset = xCurPosition;
                xCurPosition += xCodeItems[i].DetermineSize(xCurPosition);
            }
            for (int i = 0; i < xCodeItems.Count; i++) {
                if (xCodeItems[i].FileOffset != aOutput.Position) {
                    System.Diagnostics.Debugger.Break();
                }
                xCodeItems[i].WriteToStream(aOutput);
            }
        }
    }
}