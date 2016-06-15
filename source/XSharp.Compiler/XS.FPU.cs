using Cosmos.Assembler.x86.SSE;
using Cosmos.Assembler.x86.x87;
using static XSharp.Compiler.XSRegisters;

namespace XSharp.Compiler
{
  public partial class XS
  {
    public static class FPU
    {
      public static void FloatCompareAndSet(RegisterFPU register)
      {
        new FloatCompareAndSet
        {
          DestinationReg = register.RegEnum
        };
      }

      public static void FloatStoreAndPop(RegisterFPU register)
      {
        new FloatStoreAndPop
        {
          DestinationReg = register.RegEnum
        };
      }
    }
  }
}
