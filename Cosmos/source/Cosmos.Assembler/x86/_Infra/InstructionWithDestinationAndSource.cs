using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.Assembler.x86 {
    public abstract class InstructionWithDestinationAndSource : InstructionWithDestination, IInstructionWithSource {

        public InstructionWithDestinationAndSource()
        {

        }

        public InstructionWithDestinationAndSource(string mnemonic):base(mnemonic)
        {

        }

        public Cosmos.Assembler.ElementReference SourceRef {
            get;
            set;
        }

        public RegistersEnum? SourceReg
        {
            get;
            set;
        }

        public uint? SourceValue
        {
            get;
            set;
        }

        public bool SourceIsIndirect {
            get;
            set;
        }

        public int? SourceDisplacement {
            get;
            set;
        }

        public bool SourceEmpty
        {
            get;
            set;
        }
        protected string GetSourceAsString() {
            string xDest = "";
            //if ((SourceValue.HasValue || SourceRef != null) &&
            //    SourceIsIndirect && SourceReg != null) {
            //    throw new Exception("[Scale*index+base] style addressing not supported at the moment");
            //}
            if (SourceRef != null) {
                xDest = SourceRef.ToString();
            } else {
                if (SourceReg != null) {
                    xDest = Registers.GetRegisterName(SourceReg.Value);
                } else {
                    if (SourceValue.HasValue)
                        xDest = "0x" + SourceValue.GetValueOrDefault().ToString("X").ToUpperInvariant();
                }
            }
            if (SourceDisplacement != null && SourceDisplacement != 0) {
              xDest += (SourceDisplacement < 0 ? " - " : " + ") + Math.Abs(SourceDisplacement.Value);
            }
            if (SourceIsIndirect) {
                return "[" + xDest + "]";
            } else {
                return xDest;
            }
        }

        public override bool IsComplete( Cosmos.Assembler.Assembler aAssembler )
        {
            if (SourceRef != null) {
                ulong xAddress;
                return base.IsComplete(aAssembler) && SourceRef.Resolve(aAssembler, out xAddress);
            }
            return base.IsComplete(aAssembler);
        }

        public override void UpdateAddress( Cosmos.Assembler.Assembler aAssembler, ref ulong aAddress )
        {
            if (SourceRef != null) {
                SourceValue = 0xFFFFFFFF;
            }
            base.UpdateAddress(aAssembler, ref aAddress);
        }

        public override void WriteText( Cosmos.Assembler.Assembler aAssembler, System.IO.TextWriter aOutput )
        {
            aOutput.Write(mMnemonic);
            String destination=this.GetDestinationAsString();
            if (!destination.Equals("")){
                aOutput.Write(" ");
                aOutput.Write(destination);
                string source = this.GetSourceAsString();
                if (!(SourceEmpty && source.Equals(""))){
                    aOutput.Write(", ");
                    aOutput.Write(source);
                 }
            }
        }
    }
}
