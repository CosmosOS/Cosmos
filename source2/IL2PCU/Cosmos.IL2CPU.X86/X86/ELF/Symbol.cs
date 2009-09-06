using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Cosmos.IL2CPU.X86.ELF {
    public class Symbol : ELFFileContentBase {
        public override void ReadFromStream(Stream aInput) {
            Name = ReadUInt32(aInput);
            Value = ReadUInt32(aInput);
            Size = ReadUInt32(aInput);
            Info = (byte)aInput.ReadByte();
            Other = (byte)aInput.ReadByte();
            Shndx = ReadUInt16(aInput);
        }

        public override void DumpInfo(StringBuilder aOutput,
                                      string aPrefix) {
            aOutput.AppendLine("{0}Name = {1}", aPrefix, TryGetString(1,Name));
            aOutput.AppendLine("{0}Value = {1}", aPrefix, Value);
            aOutput.AppendLine("{0}Size = {1}", aPrefix, Size);
            aOutput.AppendLine("{0}Info = {1}", aPrefix, Info);
            var xInfo_Bind = Info >> 4;
            var xInfo_Type = Info & 0xF;
            aOutput.AppendLine("{0}\tBind = {1}", aPrefix, InfoBindToString(xInfo_Bind));
            aOutput.AppendLine("{0}\tType = {1}", aPrefix, InfoTypeToString(xInfo_Type));
            aOutput.AppendLine("{0}Other = {1}", aPrefix, Other);
            aOutput.AppendLine("{0}Shndx = {1}", aPrefix, Shndx);
        }

        private static string InfoBindToString(int aBind) {
            switch(aBind) {
                case 0: return "STB_LOCAL";
                case 1: return "STB_GLOBAL";
                case 2: return "STB_WEAK";
                case 13:
                case 14:
                case 15: return "STB_LOPROC/HIPROC (" + aBind + ")";
                default: return "UNKNOWN (" + aBind + ")";
            }
        }

        private static string InfoTypeToString(int aType) {
            switch(aType) {
                case 0: return "STT_NOTYPE";
                case 1: return "STT_OBJECT";
                case 2: return "STT_FUNC";
                case 3: return "STT_SECTION";
                case 4: return "STT_FILE";
                case 13:
                case 14:
                case 15: return "STT_LOPROC/HIPROC (" + aType + ")";
                default: return "UNKNOWN (" + aType + ")";
            }
        }

        public override uint DetermineSize(uint aStartAddress) {
            return 16;
        }

        public override void WriteToStream(Stream aOutput) {
            WriteUInt32(aOutput, Name);
            WriteUInt32(aOutput, Value);
            WriteUInt32(aOutput, Size);
            aOutput.WriteByte(Info);
            aOutput.WriteByte(Other);
            WriteUInt16(aOutput, Shndx);
        }

        public uint Name {
            get;
            set;
        }

        public uint Value {
            get;
            set;
        }

        public uint Size {
            get;
            set;
        }

        public byte Info {
            get;
            set;
        }

        public byte Other {
            get;
            set;
        }

        public ushort Shndx {
            get;
            set;
        }
    }
}