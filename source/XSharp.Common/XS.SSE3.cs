using Cosmos.Assembler.x86.SSE;
using static XSharp.Compiler.XSRegisters;

namespace XSharp.Compiler
{
  partial class XS
  {
    public static class SSE3
    {
      public static void MoveDoubleAndDuplicate(RegisterXMM destination, Register32 source, bool sourceIsIndirect = false)
      {
        new MoveDoubleAndDuplicate()
        {
          DestinationReg = destination,
          SourceReg = source,
          SourceIsIndirect = sourceIsIndirect
        };
      }

    }
  }
}
