namespace Cosmos.Assembler.x86
{
    /// <summary>
    /// Puts the result of the divide into EAX, and the remainder in EDX
    /// </summary>
    [Cosmos.Assembler.OpCode("div")]
    public class Divide : InstructionWithDestinationAndSize
    {
    }
}