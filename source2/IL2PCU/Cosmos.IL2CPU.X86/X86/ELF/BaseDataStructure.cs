using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Cosmos.IL2CPU.X86.ELF {
    public abstract class BaseDataStructure {
        protected uint ReadUInt32(Stream aInput) {
            var xBuff = new byte[4];
            if (aInput.Read(xBuff, 0, 4) != 4) {
                throw new Exception("Error while reading UInt32!");
            }
            return BitConverter.ToUInt32(xBuff, 0);
        }

        protected ushort ReadUInt16(Stream aInput) {
            var xBuff = new byte[2];
            if (aInput.Read(xBuff, 0, 2) != 2) {
                throw new Exception("Error while reading UInt16!");
            }
            return BitConverter.ToUInt16(xBuff, 0);
        }

        protected int ReadInt32(Stream aInput) {
            var xBuff = new byte[4];
            if (aInput.Read(xBuff, 0, 4) != 4) {
                throw new Exception("Error while reading Int32!");
            }
            return BitConverter.ToInt32(xBuff, 0);
        }

        protected void WriteUInt32(Stream aOutput, uint aValue) {
            aOutput.Write(BitConverter.GetBytes(aValue), 0, 4);
        }

        protected void WriteUInt16(Stream aOutput, ushort aValue) {
            aOutput.Write(BitConverter.GetBytes(aValue), 0, 2);
        }

        protected void WriteInt32(Stream aOutput, int aValue) {
            aOutput.Write(BitConverter.GetBytes(aValue), 0, 4);
        }

        public abstract void ReadFromStream(Stream aInput);
        public abstract void WriteToStream(Stream aOutput);

        public abstract void DumpInfo(StringBuilder aOutput, string aPrefix);


        public abstract uint DetermineSize(uint aStartAddress);
        public uint FileOffset {
            get;
            set;
        }
    }
}