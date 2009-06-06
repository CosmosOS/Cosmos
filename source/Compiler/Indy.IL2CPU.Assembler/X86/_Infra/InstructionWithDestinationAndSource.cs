using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Indy.IL2CPU.Assembler.X86 {
    public abstract class InstructionWithDestinationAndSource : InstructionWithDestination, IInstructionWithSource {
        public ElementReference SourceRef {
            get;
            set;
        }

        public RegistersEnum? SourceReg
        {
            get;
            set;
        }

        public uint? SourceValue {
            get;
            set;
        }

        public bool SourceIsIndirect {
            get;
            set;
        }

        public int SourceDisplacement {
            get;
            set;
        }

        protected string GetSourceAsString() {
            string xDest = "";
            if ((SourceValue.HasValue || SourceRef != null) &&
                SourceIsIndirect &&
                SourceReg != null) {
                throw new Exception("[Scale*index+base] style addressing not supported at the moment");
            }
            if (SourceRef != null) {
                xDest = SourceRef.ToString();
            } else {
                if (SourceReg != null) {
                    xDest = Registers.GetRegisterName(SourceReg.Value);
                } else {
                    xDest = "0x" + SourceValue.GetValueOrDefault().ToString("X").ToUpperInvariant();
                }
            }
            if (SourceDisplacement != 0) {
                xDest += " + " + SourceDisplacement;
            }
            if (SourceIsIndirect) {
                return "[" + xDest + "]";
            } else {
                return xDest;
            }
        }

        public override bool IsComplete(Indy.IL2CPU.Assembler.Assembler aAssembler) {
            if (SourceRef != null) {
                ulong xAddress;
                return base.IsComplete(aAssembler) && SourceRef.Resolve(aAssembler, out xAddress);
            }
            return base.IsComplete(aAssembler);
        }

        public override void UpdateAddress(Indy.IL2CPU.Assembler.Assembler aAssembler, ref ulong aAddress) {
            if (SourceRef != null) {
                SourceValue = 0xFFFFFFFF;
            }
            base.UpdateAddress(aAssembler, ref aAddress);
        }

        public override byte[] GetData(Indy.IL2CPU.Assembler.Assembler aAssembler) {
            if (SourceRef != null) {
                ulong xAddress = 0;
                if (!SourceRef.Resolve(aAssembler, out xAddress)) {
                    throw new Exception("Cannot resolve SourceRef!");
                }
                SourceValue = (uint)xAddress;
            }
            return base.GetData(aAssembler);
        }
        public override void WriteText(Indy.IL2CPU.Assembler.Assembler aAssembler, System.IO.TextWriter aOutput)
        {
            aOutput.Write(mMnemonic);
            aOutput.Write(" ");
            aOutput.Write(this.GetDestinationAsString());
            aOutput.Write(", ");
            aOutput.Write(this.GetSourceAsString());
        }
    }
}