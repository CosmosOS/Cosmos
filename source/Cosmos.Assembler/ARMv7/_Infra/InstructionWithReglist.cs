namespace Cosmos.Assembler.ARMv7
{
    public abstract class InstructionWithReglist : Instruction, IInstructionWithReglist
    {
        public InstructionWithReglist()
        {
        }

        public InstructionWithReglist(string mnemonic) : base(mnemonic)
        {
        }

        public RegistersEnum[] Reglist
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

            string reglist = this.GetReglistAsString();

            if (!reglist.Equals(""))
            {
                aOutput.Write(" ");
                aOutput.Write(reglist);
            }
        }
    }
}
