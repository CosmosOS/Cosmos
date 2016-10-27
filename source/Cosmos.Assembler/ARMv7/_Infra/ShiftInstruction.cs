namespace Cosmos.Assembler.ARMv7
{
    public abstract class ShiftInstruction : InstructionWithOptionalFlagsUpdateAndDestinationAndTwoOperands
    {
        public ShiftInstruction()
        {
        }

        public ShiftInstruction(string mnemonic) : base(mnemonic)
        {
        }

        public byte ShiftValue
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

            if (UpdateFlags)
            {
                aOutput.Write("S");
            }

            aOutput.Write(this.GetConditionAsString());

            string destination = this.GetDestinationAsString();
            string firstOperand = this.GetOperandAsString();
            string secondOperand = this.GetSecondOperandAsString();

            if (!destination.Equals(""))
            {
                aOutput.Write(" ");
                aOutput.Write(destination);
            }

            if (!firstOperand.Equals(""))
            {
                if (!destination.Equals(""))
                {
                    aOutput.Write(", ");
                }

                aOutput.Write(firstOperand);
            }

            if (!secondOperand.Equals(""))
            {
                if (!destination.Equals(""))
                {
                    aOutput.Write(", ");
                }

                aOutput.Write(secondOperand);
            }
            else if (ShiftValue != 0)
            {
                if (!firstOperand.Equals(""))
                {
                    aOutput.Write(", ");
                }

                aOutput.Write("#" + ShiftValue);
            }
        }
    }
}
