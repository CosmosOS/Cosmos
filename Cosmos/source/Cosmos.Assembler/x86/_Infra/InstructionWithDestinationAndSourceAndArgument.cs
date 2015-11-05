using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.Assembler.x86 {
    public abstract class InstructionWithDestinationAndSourceAndArgument : InstructionWithDestinationAndSource, IInstructionWithArgument {
		public Cosmos.Assembler.ElementReference ArgumentRef
		{
            get;
            set;
        }

		public RegistersEnum? ArgumentReg
        {
            get;
            set;
        }

		public uint? ArgumentValue
        {
            get;
            set;
        }

		public bool ArgumentIsIndirect
		{
            get;
            set;
        }

		public int ArgumentDisplacement
		{
            get;
            set;
        }

		public bool ArgumentEmpty
        {
            get;
            set;
        }

        protected string GetArgumentAsString() {
            string xDest = "";
            if ((ArgumentValue.HasValue || ArgumentRef != null) &&
                ArgumentIsIndirect &&
                ArgumentReg != null) {
                throw new Exception("[Scale*index+base] style addressing not supported at the moment");
            }
            if (ArgumentRef != null) {
                xDest = ArgumentRef.ToString();
            } else {
                if (ArgumentReg != null) {
                    xDest = Registers.GetRegisterName(ArgumentReg.Value);
                } else {
                    if (ArgumentValue.HasValue)
                        xDest = "0x" + ArgumentValue.GetValueOrDefault().ToString("X").ToUpperInvariant();
                }
            }
            if (ArgumentDisplacement != 0) {
                xDest += " + " + ArgumentDisplacement;
            }
            if (ArgumentIsIndirect) {
                return "[" + xDest + "]";
            } else {
                return xDest;
            }
        }

        public override bool IsComplete( Cosmos.Assembler.Assembler aAssembler )
        {
            if (ArgumentRef != null) {
                ulong xAddress;
                return base.IsComplete(aAssembler) && ArgumentRef.Resolve(aAssembler, out xAddress);
            }
            return base.IsComplete(aAssembler);
        }

        public override void UpdateAddress( Cosmos.Assembler.Assembler aAssembler, ref ulong aAddress )
        {
            if (ArgumentRef != null) {
                ArgumentValue = 0xFFFFFFFF;
            }
            base.UpdateAddress(aAssembler, ref aAddress);
        }

        public override void WriteText( Cosmos.Assembler.Assembler aAssembler, System.IO.TextWriter aOutput )
        {
            aOutput.Write(mMnemonic);
            String destination=this.GetDestinationAsString();
            if (!destination.Equals("")){
                aOutput.Write(' ');
                aOutput.Write(destination);
                string source = this.GetSourceAsString();
                if (!(SourceEmpty && source.Equals(""))){
                    aOutput.Write(", ");
                    aOutput.Write(source);
					string argument = this.GetArgumentAsString();
					if (!(ArgumentEmpty && argument.Equals("")))
					{
						aOutput.Write(", ");
						aOutput.Write(argument);
					}
                 }
            }
        }
    }
}