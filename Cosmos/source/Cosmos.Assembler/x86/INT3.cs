namespace Cosmos.Assembler.x86 {

  // Int 0x01 and Int 0x03 have short hand op codes.
  // For Int 0x03, the short version acts differently than the long version.
  // Intel and other asms automatically optimize the long mnemonic to the
  // short op code. NASM doesn't so we have to issue a seperate op code for it.
  // We could optimize our C# code to issue a different ouput for NASM, but there
  // are cases where the long form could be preferred. Thus instead we have 
  // chosen to follow the NASM model in our code.
  [Cosmos.Assembler.OpCode("Int3 ; INT3")]
  public class INT3 : Instruction {
  }

}