namespace Cosmos.Assembler.ARMv7
{
    public abstract class InstructionWithDestination : Instruction, IInstructionWithDestination
    {
        public InstructionWithDestination()
        {

        }

        public InstructionWithDestination(string mnemonic) : base(mnemonic)
        {

        }

        public RegistersEnum? DestinationReg
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

            string destination = this.GetDestinationAsString();

            if (!destination.Equals(""))
            {
                aOutput.Write(" ");
                aOutput.Write(destination);
            }
        }
    }
}
