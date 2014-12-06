﻿namespace Cosmos.Assembler.x86
{
    [Cosmos.Assembler.OpCode("in")]
    public class IN : InstructionWithDestinationAndSize
    {
        public override void WriteText(Cosmos.Assembler.Assembler aAssembler, System.IO.TextWriter aOutput)
        {
            base.WriteText(aAssembler, aOutput);
            aOutput.Write(", DX");
        }
    }
}