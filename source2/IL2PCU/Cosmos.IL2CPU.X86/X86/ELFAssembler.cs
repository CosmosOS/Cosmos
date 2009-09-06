using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Cosmos.IL2CPU.X86 {
    public class ELFAssembler: CosmosAssembler {
        
        public ELFAssembler() : base( 0 ) { }
 
        protected override void InitILOps()
        {
            InitILOps( typeof( ILOp ) );
        }
        public override void FlushText(System.IO.TextWriter aOutput) {
            // dont call base
            BeforeFlush();
            if (DataMembers.Count > 0) {
                aOutput.WriteLine("section .data");
                foreach (DataMember xMember in DataMembers) {
                    aOutput.WriteLine("\t" + xMember);
                }
                aOutput.WriteLine();
            }
            if (Instructions.Count > 0) {
                aOutput.WriteLine("section .text");
                string xMainLabel = "";
                for (int i = 0; i < Instructions.Count; i++) {
                    //foreach (Instruction x in mInstructions) {
                    var x = mInstructions[i];
                    string prefix = "\t\t\t";
                    Label xLabel = x as Label;
                    if (xLabel != null) {
                        string xFullName;
                        if (xLabel.Name[0] != '.') {
                            xMainLabel = xLabel.Name;
                            xFullName = xMainLabel;
                        } else {
                            xFullName = xMainLabel + xLabel.Name;
                        }
                        aOutput.WriteLine();
                        if (x.ToString()[0] == '.') {
                            prefix = "\t\t";
                        } else {
                            prefix = "\t";
                        }
                        if (xLabel.IsGlobal) {
                            aOutput.WriteLine(prefix + "global " + Label.FilterStringForIncorrectChars(xFullName));
                        }
                        aOutput.WriteLine(prefix + Label.FilterStringForIncorrectChars(xFullName) + ":");
                        continue;
                    }
                    aOutput.WriteLine(prefix + x);
                }
            }
        }

        public Label StartLabel {
            get;
            set;
        }
        
        public override void FlushBinary(System.IO.Stream aOutput, ulong aBaseAddress) {
            BeforeFlush();

            if (StartLabel == null) {
                throw new NotSupportedException("Modules not supported yet!");
            }

            ulong xCurrentAddresssCode = 0x200000;
            foreach (var xCodeItem in Instructions) {
                xCodeItem.UpdateAddress(this, ref xCurrentAddresssCode);
            }
            ulong xCurrentAddresssData = 0x800000;
            foreach(var xDataItem in DataMembers){
                xDataItem.UpdateAddress(this, ref xCurrentAddresssData);
            }

            var xFile = new ELF.File();
            // page 75
            var xCodeHeader = new ELF.SectionHeader();
            xCodeHeader.Name = xFile.StringTables[0].GetStringIndex(".text"); ;
            xCodeHeader.Address = 0x200000;
            xCodeHeader.Size = (uint)(xCurrentAddresssData - 0x200000);
            xCodeHeader.Flags = (uint)(ELF.SectionHeader.SHT_FlagsEnum.SHF_ALLOC | ELF.SectionHeader.SHT_FlagsEnum.SHF_EXECINSTR);
            var xCode = new ELF.BinarySection();
            xCodeHeader.ActualContent = xCode;
            using (var xMemStream = new MemoryStream()) {
                foreach (var xCodeItem in Instructions) {
                    xCodeItem.WriteData(this, xMemStream);
                }
                xCode.Data = xMemStream.ToArray();
            }
            xCode.SetMemoryInfo(0x200000, (uint)xCode.Data.Length);
            xFile.SectionHeaders.Add(xCodeHeader);
            xFile.BinarySections.Add(xCode);
            var xProgHeaderCode = new ELF.ProgramHeaderEntry();
            xProgHeaderCode.ActualContent = xCode;
            xProgHeaderCode.File = xFile;
            xProgHeaderCode.Align = 4096;
            xProgHeaderCode.Flags = ELF.ProgramHeaderEntry.FlagsEnum.Executable | ELF.ProgramHeaderEntry.FlagsEnum.Readable;
            xFile.ProgramHeaders.Add(xProgHeaderCode);

            var xDataHeader = new ELF.SectionHeader();
            xDataHeader.Name = xFile.StringTables[0].GetStringIndex(".data"); ;
            xDataHeader.Address = 0x800000;
            xDataHeader.Flags = (uint)(ELF.SectionHeader.SHT_FlagsEnum.SHF_ALLOC | ELF.SectionHeader.SHT_FlagsEnum.SHF_WRITE);
            xDataHeader.Size = (uint)(xCurrentAddresssData - 0x800000);
            var xData = new ELF.BinarySection();
            xDataHeader.ActualContent = xData;
            using (var xMemStream = new MemoryStream()) {
                foreach (var xDataItem in DataMembers) {
                    xDataItem.WriteData(this, xMemStream);
                }
                xData.Data = xMemStream.ToArray();
            }
            xData.SetMemoryInfo(0x800000, (uint)xData.Data.Length);
            xFile.SectionHeaders.Add(xDataHeader);
            xFile.BinarySections.Add(xData);
            var xProgHeaderData = new ELF.ProgramHeaderEntry();
            xProgHeaderData.ActualContent = xData;
            xProgHeaderData.File = xFile;
            
            xProgHeaderData.Align = 4096;
            xProgHeaderData.Flags = ELF.ProgramHeaderEntry.FlagsEnum.Writeable | ELF.ProgramHeaderEntry.FlagsEnum.Readable;
            xFile.ProgramHeaders.Add(xProgHeaderData);
            
            //foreach (var xSectionHeader in xFile.SectionHeaders) {
            //    if (xSectionHeader.ActualContent == null) {
            //        continue;
            //    }
            //    xFile.ProgramHeaders.Add(new ELF.ProgramHeaderEntry() {
            //        File = xFile,
            //        ActualContent = xSectionHeader.ActualContent
            //    });
            //}
            xFile.Header.Entry = (uint)StartLabel.ActualAddress.Value;
            xFile.WriteToStream(aOutput);
            //aOutput.SetLength(aOutput.Length + (long)(xCurrentAddresss - aBaseAddress));
            //foreach (var xItem in mAllAssemblerElements) {
            //    if (!xItem.IsComplete(this)) {
            //        throw new Exception("Incomplete element encountered.");
            //    }
            //    //var xBuff = xItem.GetData(this);
            //    //aOutput.Write(xBuff, 0, xBuff.Length);
            //    xItem.WriteData(this, aOutput);
            //}
        }

        private void WriteUInt16(Stream aOutput, ushort aValue) {
            aOutput.Write(BitConverter.GetBytes(aValue), 0, 2);
        }

        private void WriteUInt32(Stream aOutput, uint aValue) {
            aOutput.Write(BitConverter.GetBytes(aValue), 0, 4);
        }
    }
}