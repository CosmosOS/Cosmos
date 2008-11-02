using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Indy.IL2CPU.Assembler.X86 {
	[OpCode(0xFFFFFFFF, "mov")]
	public class Move: Instruction {
		public readonly string Destination;
		public readonly string Source;
		public readonly string Size;

        public Move(string aSize, string aDestination, string aSource)
            : this(aDestination, aSource) {
            Size = aSize;
            int xTempInt;
            if(Int32.TryParse(aSource, out xTempInt)) {
                SourceAsUInt = (uint)xTempInt;
            }else {
                uint xTempUInt;
                if(UInt32.TryParse(aSource, out xTempUInt)) {
                    SourceAsUInt = xTempUInt;
                }
            }
        }

        public Move(string aSize, string aDestination, UInt32 aSource)
            : this(aDestination, aSource) {
            Size = aSize;
            SourceAsUInt = aSource;
        }

        public Move(string aDestination, string aSource) {
            Destination = aDestination;
            Source = aSource;
            int xTempInt;
            if (Int32.TryParse(aSource, out xTempInt)) {
                SourceAsUInt = (uint)xTempInt;
            } else {
                uint xTempUInt;
                if (UInt32.TryParse(aSource, out xTempUInt)) {
                    SourceAsUInt = xTempUInt;
                }
            }
        }

        public Move(string aDestination, UInt32 aSource) {
            Destination = aDestination;
            Source = aSource.ToString();
            SourceAsUInt = aSource;
        }

        public Move(string aDestination, Int32 aSource) {
            Destination = aDestination;
            Source = aSource.ToString();
            SourceAsUInt = (uint)aSource;
        }

	    private uint? SourceAsUInt;
	    private byte[] xData;
        public override bool DetermineSize(Indy.IL2CPU.Assembler.Assembler aAssembler, out ulong aSize) {
            aSize = 0;
            if(Destination.Equals("eax", StringComparison.InvariantCultureIgnoreCase)) {
                if(SourceAsUInt.HasValue) {
                    aSize = 6;
                    return true;
                }
            }
            return false;
        }

        public override bool IsComplete(Indy.IL2CPU.Assembler.Assembler aAssembler) {
            if (Destination.Equals("eax", StringComparison.InvariantCultureIgnoreCase)) {
                if (SourceAsUInt.HasValue) {
                    return true;
                }
            }
            return false;
        }

        public override byte[] GetData(Indy.IL2CPU.Assembler.Assembler aAssembler) {
            var xReturn = new byte[6];
            // todo: add support other variants
            xReturn[0] = 0x66; // we want 32bit 
            xReturn[1] = 0xB8; // "mov ax"
            var xTemp = BitConverter.GetBytes(SourceAsUInt.Value);
            Array.Copy(xTemp, 0, xReturn, 2, 4);
            return xReturn;
        }

        public override string ToString() {
			if (String.IsNullOrEmpty(Size)) {
				return "mov " + Destination + ", " + Source;
			} else {
				return "mov " + Size + " " + Destination + ", " + Source;
			}
		}
	}
}
