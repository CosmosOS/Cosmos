using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.IL2CPU.X86.ELF {
    public class ProgramHeaderEntry : ELFFileContentBase {
        [Flags]
        public enum FlagsEnum : uint {
            Executable = 0x1,
            Writeable = 0x2,
            Readable = 0x4
        }

        public enum TypeEnum : uint {
            Null = 0x0,
            Load = 0x1,
            Dynamic = 0x2,
            Interp = 0x3,
            Note = 4,
            SHLib = 5,
            ProgramHeaderTable = 6
        }
        public ProgramHeaderEntry() {
            Type = TypeEnum.Load;
        }

        public override uint DetermineSize(uint aStartAddress) {
            return 32;
        }

        public override void DumpInfo(StringBuilder aOutput, string aPrefix) {
            throw new NotImplementedException();
        }

        public override void ReadFromStream(System.IO.Stream aInput) {
            throw new NotImplementedException();
        }

        public BaseSection ActualContent {
            get;
            set;
        }

        public override void WriteToStream(System.IO.Stream aOutput) {
            if (ActualContent == null) {
                throw new Exception("No actual content for program header!");
            }
            switch (Type) {
                case TypeEnum.Load:
                    Offset = ActualContent.FileOffset;
                    VAddress = ActualContent.MemoryOffset;
                    PAddress = ActualContent.MemoryOffset;
                    Size = ActualContent.MemorySize;
                    MemorySize = ActualContent.MemorySize;
                    break;
            }

            //Align = 4;
            WriteUInt32(aOutput, (uint)Type);
            WriteUInt32(aOutput, Offset);
            WriteUInt32(aOutput, VAddress);
            WriteUInt32(aOutput, PAddress);
            WriteUInt32(aOutput, Size);
            WriteUInt32(aOutput, MemorySize);
            WriteUInt32(aOutput, (uint)Flags);
            WriteUInt32(aOutput, Align);
        }

        /// <summary>
        /// Segment kind
        /// </summary>
        public TypeEnum Type {
            get;
            set;
        }

        /// <summary>
        /// Byte offset in this file
        /// </summary>
        public uint Offset {
            get;
            set;
        }

        /// <summary>
        /// Virtual address in memory of first byte
        /// </summary>
        public uint VAddress {
            get;
            set;
        }

        /// <summary>
        /// Physical address in memory, if applicable
        /// </summary>
        public uint PAddress{
            get;
            set;
        }

        /// <summary>
        /// Size of segment in file
        /// </summary>
        public uint Size {
            get;
            set;
        }

        /// <summary>
        /// Size of segment in memory
        /// </summary>
        public uint MemorySize {
            get;
            set;
        }

        /// <summary>
        /// Flags relevant to the segment
        /// </summary>
        public FlagsEnum Flags {
            get;
            set;
        }

        
        /// <summary>
        /// Data alignment
        /// </summary>
        public uint Align {
            get;
            set;
        }
    }
}
