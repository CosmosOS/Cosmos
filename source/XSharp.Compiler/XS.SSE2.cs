using System;
using Cosmos.Assembler.x86;
using Cosmos.Assembler.x86.SSE;
using static XSharp.Compiler.XSRegisters;

namespace XSharp.Compiler
{
  partial class XS
  {
    public static class SSE2
    {
      public static void ConvertSD2SIAndTruncate(Register32 destination, RegisterXMM source)
      {
        new ConvertSD2SIAndTruncate
        {
          DestinationReg = destination,
          SourceReg = source
        };
      }

      public static void MoveD(string destination, Register source, bool destinationIsIndirect = false, int? destinationDisplacement = null, bool sourceIsIndirect = false, int? sourceDisplacement = null)
      {
        DoDestinationSource<MoveD>(destination, source, destinationIsIndirect, destinationDisplacement, sourceIsIndirect, sourceDisplacement);
      }

      public static void MoveD(string destination, UInt32 value, bool destinationIsIndirect = false, int? destinationDisplacement = null, bool sourceIsIndirect = false, int? sourceDisplacement = null)
      {
        DoDestinationSource<MoveD>(destination, value, destinationIsIndirect, destinationDisplacement, sourceIsIndirect, sourceDisplacement);
      }

      public static void MoveD(string destination, string source, bool destinationIsIndirect = false, int? destinationDisplacement = null, bool sourceIsIndirect = false, int? sourceDisplacement = null)
      {
        DoDestinationSource<MoveD>(destination, source, destinationIsIndirect, destinationDisplacement, sourceIsIndirect, sourceDisplacement);
      }

      public static void MoveD(Register destination, string sourceLabel, bool destinationIsIndirect = false, int? destinationDisplacement = null, bool sourceIsIndirect = false, int? sourceDisplacement = null)
      {
        DoDestinationSource<MoveD>(destination, sourceLabel, destinationIsIndirect, destinationDisplacement, sourceIsIndirect, sourceDisplacement);
      }

      public static void MoveD(Register destination, uint value, bool destinationIsIndirect = false, int? destinationDisplacement = null, bool sourceIsIndirect = false, int? sourceDisplacement = null)
      {
        DoDestinationSource<MoveD>(destination, value, destinationIsIndirect, destinationDisplacement, sourceIsIndirect, sourceDisplacement);
      }

      public static void MoveD(Register destination, Register source, bool destinationIsIndirect = false, int? destinationDisplacement = null, bool sourceIsIndirect = false, int? sourceDisplacement = null)
      {
        DoDestinationSource<MoveD>(destination, source, destinationIsIndirect, destinationDisplacement, sourceIsIndirect, sourceDisplacement);
      }

    }
  }
}
