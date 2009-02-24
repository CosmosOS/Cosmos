using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Indy.IL2CPU.Assembler.X86 {
    [OpCode("jmp")]
    public class JumpToSegment : IL2CPU.Assembler.Instruction {
        public ElementReference DestinationRef {
            get;
            set;
        }

        public ushort Segment {
            get;
            set;
        }

        public override string ToString() {
            if (DestinationRef != null) {
                return "jmp " + Segment + ":" + DestinationRef.ToString();
            } else {
                return "jmp " + Segment + ":0x0";
            }
        }

        public string DestinationLabel {
            get {
                if (DestinationRef != null) {
                    return DestinationRef.Name;
                }
                return String.Empty;
            }
            set {
                DestinationRef = new ElementReference(value);
            }
        }

        public override bool IsComplete(Indy.IL2CPU.Assembler.Assembler aAssembler) {
            ulong xAddress;
            return DestinationRef == null || DestinationRef.Resolve(aAssembler, out xAddress);
        }

        public override void UpdateAddress(Indy.IL2CPU.Assembler.Assembler aAssembler, ref ulong aAddress) {
            base.UpdateAddress(aAssembler, ref aAddress);
            aAddress += 7;
        }

        //public override byte[] GetData(Indy.IL2CPU.Assembler.Assembler aAssembler) {
        public override void WriteData(Indy.IL2CPU.Assembler.Assembler aAssembler, System.IO.Stream aOutput) {
            aOutput.WriteByte(0xEA);
            ulong xAddress = 0;
            if (DestinationRef != null && DestinationRef.Resolve(aAssembler, out xAddress)) {
                xAddress = (ulong)(((long)xAddress) + DestinationRef.Offset);
            }
            aOutput.Write(BitConverter.GetBytes((uint)(xAddress)), 0, 4);
            aOutput.Write(BitConverter.GetBytes(Segment), 0, 2);
        }
    }
}