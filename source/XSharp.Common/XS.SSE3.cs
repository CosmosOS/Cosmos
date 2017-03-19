using Cosmos.Assembler.x86.SSE;

namespace XSharp.Common
{
  partial class XS
  {
    public static class SSE3
    {
      public static void MoveDoubleAndDuplicate(XSRegisters.RegisterXMM destination, XSRegisters.Register32 source, bool sourceIsIndirect = false)
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
