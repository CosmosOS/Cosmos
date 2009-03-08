using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Indy.IL2CPU.Assembler.X86.ELF {
    public class StringTable : BaseSection {
        private SectionHeader mHeader;
        public StringTable(SectionHeader aSectionHeader) {
            mHeader = aSectionHeader;
            mStrings = new Dictionary<uint, string>();
            aSectionHeader.Type = SectionHeader.SHT_STRTAB;
        }
        public override void ReadFromStream(Stream aInput) {
            if (aInput.ReadByte() != 0) {
                throw new Exception("StringTable should start with byte #0");
            }
            var xSB = new StringBuilder();
            mStrings.Clear();
            var xPreviousIndex = aInput.Position - mHeader.Offset;
            while (aInput.Position < (mHeader.Offset + mHeader.Size)) {
                var xByte = aInput.ReadByte();
                if (xByte <= 0) {
                    mStrings.Add((uint)(xPreviousIndex), xSB.ToString());
                    xPreviousIndex = aInput.Position - mHeader.Offset;
                    xSB.Remove(0, xSB.Length);
                    continue;
                }
                xSB.Append((char)xByte);
            }
        }

        public override uint MemoryOffset {
            get { return 0; }
        }

        public override uint MemorySize {
            get { return 0; }
        }
        
        public override void WriteToStream(Stream aOutput) {
            aOutput.WriteByte(0);
            foreach (var xString in mStrings.Values) {
                foreach (var xChar in xString) {
                    aOutput.WriteByte((byte)xChar);
                }
                aOutput.WriteByte(0);
            }
            aOutput.WriteByte(0);
        }

        private Dictionary<uint, string> mStrings;
        //public IEnumerable<string> Strings {
        //    get {
        //        return mStrings;
        //    }
        //}

        public string GetString(uint aIndex) {
            return mStrings[aIndex];
        }

        public uint GetStringIndex(string aString) {
            if (mStrings.ContainsValue(aString)) {
                return (from item in mStrings
                        where item.Value == aString
                        select item.Key).Single();
            }
            var xLast = mStrings.LastOrDefault();
            if (xLast.Key != 0) {
                mStrings.Add((uint)(xLast.Key + 1 + xLast.Value.Length), aString);
                return (uint)(xLast.Key + 1 + xLast.Value.Length);
            } else {
                mStrings.Add(1, aString);
                return 1;
            }
            return 0;
        }

        public override void DumpInfo(StringBuilder aOutput,
                                      string aPrefix) {
            foreach (var xString in mStrings) {
                aOutput.AppendLine("{0}Strings[{1}] = '{2}'", aPrefix, xString.Key, xString.Value);
            }
        }

        public override uint DetermineSize(uint aStartAddress) {
            if (mStrings.Count > 0) {
                return (uint)(mStrings.Last().Key + 2 + mStrings.Last().Value.Length);
            } else {
                return 2;
            }
        }
    }
}