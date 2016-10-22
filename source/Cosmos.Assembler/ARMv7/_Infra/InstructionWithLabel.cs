namespace Cosmos.Assembler.ARMv7
{
    public abstract class InstructionWithLabel : Instruction, IInstructionWithLabel
    {
        public InstructionWithLabel()
        {
        }

        public InstructionWithLabel(string mnemonic) : base(mnemonic)
        {
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
