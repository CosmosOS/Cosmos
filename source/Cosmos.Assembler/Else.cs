namespace Cosmos.Assembler
{
    [Cosmos.Assembler.OpCode("%else")]
    public class Else : Instruction
    {
        public override void WriteText(Cosmos.Assembler.Assembler aAssembler, System.IO.TextWriter aOutput)
        {
            aOutput.Write(Mnemonic);
        }
    }
}