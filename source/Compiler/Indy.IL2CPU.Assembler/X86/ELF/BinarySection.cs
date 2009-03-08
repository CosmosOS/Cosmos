using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Indy.IL2CPU.Assembler.X86.ELF {
    public class BinarySection : BaseSection {
        public BinarySection() {
            //
        }

        private uint mMemoryOffset;
        private uint mMemorySize;
        public override uint MemoryOffset {
            get {
                return mMemoryOffset;
            }
        }

        public override uint MemorySize {
            get {
                return mMemorySize;
            }
        }

        public void SetMemoryInfo(uint mOffset, uint mSize) {
            mMemoryOffset = mOffset;
            mMemorySize = mSize;
        }

        public byte[] Data {
            get;
            set;
        }

        public override uint DetermineSize(uint aStartAddress) {
            return (uint)Data.Length;
        }

        public override void ReadFromStream(System.IO.Stream aInput) {
            throw new NotImplementedException();
        }

        public override void WriteToStream(System.IO.Stream aOutput) {
            aOutput.Write(Data, 0, Data.Length);
        }

        public override void DumpInfo(StringBuilder aOutput, string aPrefix) {
            //
        }
    }
}