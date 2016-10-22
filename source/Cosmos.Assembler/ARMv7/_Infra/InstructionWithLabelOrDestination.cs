namespace Cosmos.Assembler.ARMv7
{
    public abstract class InstructionWithLabelOrDestination : InstructionWithDestination, IInstructionWithLabel
    {
        public InstructionWithLabelOrDestination()
        {
        }

        public InstructionWithLabelOrDestination(string mnemonic) : base(mnemonic)
        {
        }

        public RegistersEnum? FirstOperandReg
        {
            get;
            set;
        }

        public string Label
        {
            get;
            set;
        }

        public uint? LabelOffset
        {
            get;
            set;
        }

        public override bool IsComplete(Assembler aAssembler)
        {
            return base.IsComplete(aAssembler);
        }

        public override void UpdateAddress(Assembler aAssembler, ref ulong aAddress)
        {
            base.UpdateAddress(aAssembler, ref aAddress);
        }

        public override void WriteText(Assembler aAssembler, System.IO.TextWriter aOutput)
        {
            if(this.GetDestinationAsString() != "")
            {
                base.WriteText(aAssembler, aOutput);
            }
            else
            {
                aOutput.Write(mMnemonic);

                string label = this.GetLabelAsString();

                if (!string.IsNullOrEmpty(label))
                {
                    aOutput.Write(" ");
                    aOutput.Write(label);
                }
            }
        }
    }
}
