namespace Cosmos.Compiler.Assembler.X86 {

  // See note in Int3 as to why we need a separate op for Int1 versus Int 0x01
  [OpCode("Int1")]
	public class Int1: Instruction { 
  }

}
