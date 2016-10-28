namespace Cosmos.Assembler.ARMv7
{
    [OpCode("LDR")]
    public class LoadRegister : InstructionWithDataSizeAndOperandsAndMemoryAddress, IInstructionWithLabel
    {
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

        public LoadRegister() : base("LDR")
        {
        }

        public override void WriteText(Assembler aAssembler, System.IO.TextWriter aOutput)
        {
            var label = this.GetLabelAsString();

            if (label.Equals(""))
            {
                base.WriteText(aAssembler, aOutput);
            }
            else
            {
                aOutput.Write(mMnemonic);
                aOutput.Write(this.GetDataSizeAsString());
                aOutput.Write(this.GetConditionAsString());

                string firstOperand = this.GetOperandAsString();
                string secondOperand = this.GetSecondOperandAsString();

                if(secondOperand.Equals(""))
                {
                    aOutput.Write(" ");
                    aOutput.Write(firstOperand);

                    aOutput.Write(", ");
                    aOutput.Write(label);
                }
                else
                {
                    aOutput.Write(" ");
                    aOutput.Write(firstOperand);

                    aOutput.Write(", ");
                    aOutput.Write(secondOperand);

                    aOutput.Write(", ");
                    aOutput.Write(label);
                }
            }
        }
    }
}
