namespace Cosmos.Assembler.x86
{
    [Cosmos.Assembler.OpCode("movsx")]
    public class MoveSignExtend : InstructionWithDestinationAndSource
    {

        public override void WriteText(Cosmos.Assembler.Assembler aAssembler, System.IO.TextWriter aOutput)
        {
            aOutput.Write(mMnemonic);
            if (!DestinationEmpty)
            {
                aOutput.Write(" ");
                aOutput.Write(this.GetDestinationAsString());
                aOutput.Write(", ");
                aOutput.Write(" ");
                aOutput.Write(this.GetSourceAsString());
            }
        }
    }
}