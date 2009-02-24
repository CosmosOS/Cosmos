using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Indy.IL2CPU.Assembler.X86 {
    public abstract class InstructionWithDestination : Instruction, IInstructionWithDestination {
        public ElementReference DestinationRef {
            get;
            set;
        }

        public Guid DestinationReg {
            get;
            set;
        }

        public uint? DestinationValue {
            get;
            set;
        }

        public bool DestinationIsIndirect {
            get;
            set;
        }

        public int DestinationDisplacement {
            get;
            set;
        }

        public override bool IsComplete(Indy.IL2CPU.Assembler.Assembler aAssembler) {
            if (DestinationRef != null) {
                ulong xAddress;
                return base.IsComplete(aAssembler) && DestinationRef.Resolve(aAssembler, out xAddress);
            }
            return base.IsComplete(aAssembler);
        }

        public override void UpdateAddress(Indy.IL2CPU.Assembler.Assembler aAssembler, ref ulong aAddresss) {
            if (DestinationRef != null) {
                DestinationValue = 0xFFFFFFFF;
            }
            base.UpdateAddress(aAssembler, ref aAddresss);
        }


        public override byte[] GetData(Indy.IL2CPU.Assembler.Assembler aAssembler) {
            if (DestinationRef != null) {
                ulong xAddress = 0;
                if (!DestinationRef.Resolve(aAssembler, out xAddress)) {
                    throw new Exception("Cannot resolve DestinationRef!");
                }
                DestinationValue = (uint)xAddress;
            }
            return base.GetData(aAssembler);
        }

        public override string ToString() {
            return base.mMnemonic + " " + this.GetDestinationAsString();
        }
    }
}