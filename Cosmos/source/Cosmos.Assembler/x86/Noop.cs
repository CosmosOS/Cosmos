namespace Cosmos.Assembler.x86
{
    [Cosmos.Assembler.OpCode("NOP")]
    public class Noop : Instruction
    {
    }

    [Cosmos.Assembler.OpCode("NOP ; INT3")]
    public class DebugNoop : Instruction
    {
    }
}