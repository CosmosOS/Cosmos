using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Cosmos.IL2CPU.X86.ELF {
    public class SectionHeader : ELFFileContentBase {
        public const int SHT_SYMTAB = 2;
        public const int SHT_STRTAB = 3;
        public enum SHT_Type: uint {
            SHT_NULL = 0,
            SHT_PROGBITS = 1,
            SHT_SYMTAB =2,
            SHT_STRTAB = 3,
            SHT_RELA=4,
            SHT_HASH = 5,
            SHT_DYNAMIC = 6,
            SHT_NOTE = 7,
            SHT_NOBITS = 8,
            SHT_REL = 9,
            SHT_SHLIB = 10,
            SHT_DYNSYM = 11
        }
        [Flags]
        public enum SHT_FlagsEnum: uint {
            SHF_WRITE = 0x1,
            SHF_ALLOC = 0x2,
            SHF_EXECINSTR = 0x4,
            SHF_MASKPROC = 0xf0000000
        }
        public override void ReadFromStream(Stream aInput) {
            Name = ReadUInt32(aInput);
            Type = ReadUInt32(aInput);
            Flags = ReadUInt32(aInput);
            Address = ReadUInt32(aInput);
            Offset = ReadUInt32(aInput);
            Size = ReadUInt32(aInput);
            Link = ReadUInt32(aInput);
            Info = ReadUInt32(aInput);
            AddrAlign = ReadUInt32(aInput);
            EntSize = ReadUInt32(aInput);
        }

        public override void WriteToStream(Stream aOutput) {
            if (ActualContent == null && File.SectionHeaders[0] != this) {
                throw new Exception("Content of section '" + this + "' not specified!");
            }
            WriteUInt32(aOutput, Name);
            if (ActualContent != null) {
                if (ActualContent is SymbolTable) {
                    Type = (uint)SHT_Type.SHT_SYMTAB;
                    Size = ActualContent.MemorySize;
                } else {
                    if (ActualContent is StringTable) {
                        Type = (uint)SHT_Type.SHT_STRTAB;
                        Size = ActualContent.DetermineSize(0);
                    } else {
                        Type = (uint)SHT_Type.SHT_PROGBITS;
                        Size = ActualContent.MemorySize;
                    }
                }
                Address = ActualContent.MemoryOffset;
                //Size = ActualContent.MemorySize;
                Offset = ActualContent.FileOffset;
            }
            WriteUInt32(aOutput, Type);
            WriteUInt32(aOutput, Flags);
            WriteUInt32(aOutput, Address);
            WriteUInt32(aOutput, Offset);
            WriteUInt32(aOutput, Size);
            WriteUInt32(aOutput, Link);
            WriteUInt32(aOutput, Info);
            WriteUInt32(aOutput, AddrAlign);
            WriteUInt32(aOutput, EntSize);
        }

        public override void DumpInfo(StringBuilder aOutput,
                                      string aPrefix) {
            aOutput.AppendLine("{0}Name: {1}", aPrefix, TryGetString(0,Name)); // improve: hardcoded zero is correct?
            aOutput.AppendLine("{0}Type: {1}", aPrefix, TypeToString(Type));
            aOutput.AppendLine("{0}Flags: {1} ({2})", aPrefix, ((SHT_FlagsEnum)Flags), Flags.ToString("X"));
            aOutput.AppendLine("{0}Address: {1}", aPrefix, Address);
            aOutput.AppendLine("{0}Offset: {1}", aPrefix, Offset);
            aOutput.AppendLine("{0}Size: {1}", aPrefix, Size);
            aOutput.AppendLine("{0}Link: {1}", aPrefix, Link);
            aOutput.AppendLine("{0}Info: {1}", aPrefix, Info);
            aOutput.AppendLine("{0}AddrAlign: {1}", aPrefix, AddrAlign);
            aOutput.AppendLine("{0}EntSize: {1}", aPrefix, EntSize);
        }

        public override uint DetermineSize(uint aStartAddress) {
            return 40;
        }

        /// <summary>
        /// Name of the section
        /// </summary>
        public uint Name {
            get;
            set;
        }

        /// <summary>
        /// Type of the section
        /// </summary>
        public uint Type {
            get;
            set;
        }

        /// <summary>
        /// Flags describing miscellaneous attributes
        /// </summary>
        public uint Flags {
            get;
            set;
        }

        /// <summary>
        /// The memory address of the start of the data in this section, if in memory
        /// </summary>
        public uint Address {
            get;
            set;
        }

        /// <summary>
        /// byte offset of this section in the file.
        /// </summary>
        public uint Offset {
            get;
            set;
        }

        /// <summary>
        /// the size of the section
        /// </summary>
        public uint Size {
            get;
            set;
        }

        /// <summary>
        /// 
        /// </summary>
        public uint Link {
            get;
            set;
        }

        /// <summary>
        /// Extra information
        /// </summary>
        public uint Info {
            get;
            set;
        }

        /// <summary>
        /// Address alignment
        /// </summary>
        public uint AddrAlign {
            get;
            set;
        }

        /// <summary>
        /// The size of each entry in this section (if fixed size)
        /// </summary>
        public uint EntSize {
            get;
            set;
        }

        private static string TypeToString(uint aType) {
            switch (aType) {
                case 0: return "SHT_NULL";
                case 1: return "SHT_PROGBITS";
                case SHT_SYMTAB: return "SHT_SYMTAB";
                case SHT_STRTAB: return "SHT_STRTAB";
                case 4: return "SHT_RELA";
                case 5: return "SHT_HASH";
                case 6: return "SHT_DYNAMIC";
                case 7: return "SHT_NOTE";
                case 8: return "SHT_NOBITS";
                case 9: return "SHT_REL";
                case 10: return "SHT_SHLIB";
                case 11: return "SHT_DYNSYM";
                default:
                    if (aType >= 0x70000000 && aType <= 0x7fffffff) {
                        return "SHT_LOPROC/SHT_HIPROC (" + aType + ")";
                    }
                    if (aType >= 0x80000000 && aType <= 0xFfffffff) {
                        return "SHT_LOUSER/SHT_HIUSER (" + aType + ")";
                    }
                    return "UNKNOWN (" + aType + ")";
            }
        }

        public BaseSection ActualContent {
            get;
            set;
        }


    }
}