namespace Cosmos.Assembler.ARMv7
{
    public abstract class InstructionWithTwoOperands : Instruction, IInstructionWithOperand, IInstructionWithOperand2
    {

        public InstructionWithTwoOperands()
        {
        }

        public InstructionWithTwoOperands(string mnemonic) : base(mnemonic)
        {
        }

        public RegistersEnum? FirstOperandReg
        {
            get;
            set;
        }

        public RegistersEnum? SecondOperandReg
        {
            get;
            set;
        }

        public uint SecondOperandValue
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

            aOutput.Write(this.GetConditionAsString());

            string firstOperand = this.GetFirstOperandAsString();

            if (!(firstOperand.Equals("")))
            {
                aOutput.Write(" ");
                aOutput.Write(firstOperand);

                string secondOperand = this.GetSecondOperandAsString();

                if (!secondOperand.Equals(""))
                {
                    aOutput.Write(", ");
                    aOutput.Write(secondOperand);
                }
            }
        }
    }
}
