namespace Cosmos.Assembler.ARMv7
{
    public abstract class InstructionWithDestinationAndLabel : Instruction, IInstructionWithDestination, IInstructionWithLabel
    {
        public InstructionWithDestinationAndLabel()
        {

        }

        public InstructionWithDestinationAndLabel(string mnemonic) : base(mnemonic)
        {

        }

        public RegistersEnum? DestinationReg
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

        public override void UpdateAddress(Assembler aAssembler, ref ulong aAddresss)
        {
            base.UpdateAddress(aAssembler, ref aAddresss);
        }

        public override void WriteText(Assembler aAssembler, System.IO.TextWriter aOutput)
        {
            aOutput.Write(mMnemonic);

            aOutput.Write(this.GetConditionAsString());

            string destination = this.GetDestinationAsString();
            string label = this.GetLabelAsString();

            if (!destination.Equals(""))
            {
                aOutput.Write(" ");
                aOutput.Write(destination);

                if(!label.Equals(""))
                {
                    aOutput.Write(", ");
                    aOutput.Write(label);
                }
            }
        }
    }
}
