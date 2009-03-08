using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Indy.IL2CPU.Assembler.X86.ELF {
    public class Header : BaseDataStructure {
        private const int IdentSize = 16;
        public Header() {
            Ident = new byte[IdentSize] {
                0x7f, 69, 76, 70, 1, 1, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0
            };
            Type = 2;
            Machine = 3;
            Version = 1;
            EHSize = 0x34;
        }
        public File File {
            get;
            set;
        }
        public override void ReadFromStream(Stream aInput) {
            Ident = new byte[IdentSize];
            if (aInput.Read(Ident, 0, IdentSize) != IdentSize) {
                throw new Exception("Error while reading (1)");
            }
            if(Ident[0] != 0x7F) {
                throw new Exception("Ident error (0)");
            }
            if (Ident[1] != 69) {
                throw new Exception("Ident error (1)");
            }
            if (Ident[2] != 76) {
                throw new Exception("Ident error (2)");
            }
            if (Ident[3] != 70) {
                throw new Exception("Ident error (3)");
            }

            // EI_Class
            // - for now, assume 32bit
            if (Ident[4] != 1) {
                throw new Exception("Ident error (3)");
            }
            // Data encoding, for now assume LSB
            if (Ident[5] != 1) {
                throw new Exception("DataEncoding '" + Ident[5] + "' not supported!");
            }
            // ELF version, for now assume 1
            if (Ident[6] != 1) {
                throw new Exception("ELF version '" + Ident[6] + "' not supported!");
            }
            Type = ReadUInt16(aInput);
            Machine = ReadUInt16(aInput);
            Version = ReadUInt32(aInput);
            Entry = ReadUInt32(aInput);
            PhOffset = ReadUInt32(aInput);
            ShOffset = ReadUInt32(aInput);
            Flags = ReadUInt32(aInput);
            EHSize = ReadUInt16(aInput);
            PhEntSize = ReadUInt16(aInput);
            PhNum = ReadUInt16(aInput);
            ShEntSize = ReadUInt16(aInput);
            ShNum = ReadUInt16(aInput);
            ShStrNdx = ReadUInt16(aInput);
        }

        public override void WriteToStream(Stream aOutput) {
            aOutput.Write(Ident, 0, IdentSize);
            if (File == null) {
                throw new Exception("No File specified!");
            }
            if (File.ProgramHeaders.Count > 0) {
                PhOffset = File.ProgramHeaders[0].FileOffset;
                PhNum = (ushort)File.ProgramHeaders.Count;
                PhEntSize = 32;
            }
            if (File.SectionHeaders.Count > 0) {
                ShOffset = File.SectionHeaders[0].FileOffset;
                ShNum = (ushort)File.SectionHeaders.Count;
                ShEntSize = 40;
            }
            WriteUInt16(aOutput, Type);
            WriteUInt16(aOutput, Machine);
            WriteUInt32(aOutput, Version);
            WriteUInt32(aOutput, Entry);
            WriteUInt32(aOutput, PhOffset);
            WriteUInt32(aOutput, ShOffset);
            WriteUInt32(aOutput, Flags);
            WriteUInt16(aOutput, EHSize);
            WriteUInt16(aOutput, PhEntSize);
            WriteUInt16(aOutput, PhNum);
            WriteUInt16(aOutput, ShEntSize);
            WriteUInt16(aOutput, ShNum);
            WriteUInt16(aOutput, ShStrNdx);
        }

        /// <summary>
        /// ELF identification bytes
        /// </summary>
        public byte[] Ident {
            get;
            set;
        }

        /// <summary>
        /// Object file type
        /// </summary>
        public ushort Type {
            get;
            set;
        }

        /// <summary>
        /// Required architecture for this object file
        /// </summary>
        public ushort Machine {
            get;
            set;
        }

        /// <summary>
        /// Object file version
        /// </summary>
        public uint Version {
            get;
            set;
        }

        /// <summary>
        /// Entry address
        /// </summary>
        public uint Entry {
            get;
            set;
        }

        /// <summary>
        /// Offset of the program header
        /// </summary>
        public uint PhOffset {
            get;
            set;
        }

        /// <summary>
        /// Section header offset
        /// </summary>
        public uint ShOffset {
            get;
            set;
        }

        /// <summary>
        /// Processor-specific flags
        /// </summary>
        public uint Flags {
            get;
            set;
        }

        /// <summary>
        /// ELF header size
        /// </summary>
        public ushort EHSize {
            get;
            set;
        }

        /// <summary>
        /// Program header entry size
        /// </summary>
        public ushort PhEntSize {
            get;
            set;
        }

        /// <summary>
        /// Program header entry number
        /// </summary>
        public ushort PhNum {
            get;
            set;
        }

        /// <summary>
        /// Section header entry size
        /// </summary>
        public ushort ShEntSize {
            get;
            set;
        }

        /// <summary>
        /// Section header entry number
        /// </summary>
        public ushort ShNum {
            get;
            set;
        }

        /// <summary>
        /// default string table index
        /// </summary>
        public ushort ShStrNdx {
            get;
            set;
        }

        public override uint DetermineSize(uint aStartAddress) {
            return 36 + IdentSize;
        }

        public override void DumpInfo(StringBuilder aOutput, string aPrefix) {
            aOutput.AppendLine("{0}Ident: 0x{1}", aPrefix, Ident.Aggregate("", (r, b) => r + b.ToString("X2")).ToUpper());
            aOutput.AppendLine("{0}Type: {1}", aPrefix, TypeToString(Type));
            aOutput.AppendLine("{0}Machine: {1}", aPrefix, MachineToString(Machine));
            aOutput.AppendLine("{0}Version: {1}", aPrefix, Version);
            aOutput.AppendLine("{0}Entry: {1}", aPrefix, Entry);
            aOutput.AppendLine("{0}PhOffset: {1}", aPrefix, PhOffset);
            aOutput.AppendLine("{0}ShOffset: {1}", aPrefix, ShOffset);
            aOutput.AppendLine("{0}Flags: 0x{1}", aPrefix, Flags.ToString("X8"));
            aOutput.AppendLine("{0}EHSize: {1}", aPrefix, EHSize);
            aOutput.AppendLine("{0}PhEntSize: {1}", aPrefix, PhEntSize);
            aOutput.AppendLine("{0}PhNum: {1}", aPrefix, PhNum);
            aOutput.AppendLine("{0}ShEntSize: {1}", aPrefix, ShEntSize);
            aOutput.AppendLine("{0}ShNum: {1}", aPrefix, ShNum);
            aOutput.AppendLine("{0}ShStrNdx: {1}", aPrefix, ShStrNdx);
        }

        private string MachineToString(ushort aMachine) {
            switch (aMachine) {
                case 0: return "EM_NONE";
                case 1: return "EM_M32";
                case 2: return "EM_SPARC";
                case 3: return "EM_386";
                case 4: return "EM_68K";
                case 5: return "EM_88K";
                case 7: return "EM_860";
                case 8: return "EM_MIPS";
                case 10: return "EM_MIPS_RS4_BE";
                default:
                    if (aMachine >= 11 && aMachine <= 16) {
                        return "RESERVED (" + aMachine + ")";
                    }
                    return "UNKNOWN (" + aMachine + ")";
            }
        }

        private static string TypeToString(ushort aType) {
            switch (aType) {
                case 0: return "ET_NONE";
                case 1: return "ET_REL";
                case 2: return "ET_EXEC";
                case 3: return "ET_DYN";
                case 4: return "ET_CORE";
                default:
                    if (aType >= 0xFF00 && aType <= 0xFFFF) {
                        return "RESERVED (" + aType + ")";
                    } else {
                        return "UNKNOWN (" + aType + ")";
                    }
            }
        }
    }
}