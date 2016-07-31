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
      public static void AddSD(RegisterXMM destination, RegisterXMM source)
      {
        DoDestinationSource<AddSD>(destination, source);
      }

      public static void DivSD(RegisterXMM destination, RegisterXMM source)
      {
        new DivSD
        {
           DestinationReg = destination,
           SourceReg = source
        };
      }

      public static void MulSD(RegisterXMM destination, RegisterXMM source)
      {
          DoDestinationSource<MulSD>(destination, source);
      }

      public static void SubSD(RegisterXMM destination, RegisterXMM source)
      {
          DoDestinationSource<SubSD>(destination, source);
      }

      public static void CompareSD(RegisterXMM destination, RegisterXMM source, ComparePseudoOpcodes comparision)
      {
       new CompareSD()
       {
         DestinationReg = destination,
         SourceReg = source,
         pseudoOpcode = (byte)comparision
       };
      }

      public static void ConvertSD2SIAndTruncate(Register32 destination, RegisterXMM source)
      {
        new ConvertSD2SIAndTruncate
        {
          DestinationReg = destination,
          SourceReg = source
        };
      }

      public static void ConvertSD2SS(RegisterXMM destination, Register32 source, bool sourceIsIndirect = false)
      {
        new ConvertSD2SS()
        {
            DestinationReg = destination,
            SourceReg = source,
            SourceIsIndirect = sourceIsIndirect
        };
     }

     public static void ConvertSI2SD(RegisterXMM destination, Register32 source, bool sourceIsIndirect = false, int? sourceDisplacement = null, bool destinationIsIndirect = false, int? destinationDisplacement = null)
     {
         new ConvertSI2SD()
         {
            DestinationReg = destination,
            DestinationIsIndirect = destinationIsIndirect,
            DestinationDisplacement = destinationDisplacement,
            SourceReg = source,
            SourceIsIndirect = sourceIsIndirect,
            SourceDisplacement = sourceDisplacement
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

      public static void MoveSD(RegisterXMM destination, Register32 source, bool sourceIsIndirect = false)
      {
        new MoveSD()
        {
          DestinationReg = destination,
          SourceReg = source,
          SourceIsIndirect = sourceIsIndirect
        };
      }

      public static void MoveSD(Register32 destination, RegisterXMM source, bool destinationIsIndirect = false)
      {
        new MoveSD()
        {
          DestinationReg = destination,
          DestinationIsIndirect = destinationIsIndirect,
          SourceReg = source
        };
      }

      public static void XorPD(RegisterXMM destination, RegisterXMM source, bool destinationIsIndirect = false, int? destinationDisplacement = null, bool sourceIsIndirect = false, int? sourceDisplacement = null)
      {
          DoDestinationSource<XorPD>(destination, source, destinationIsIndirect, destinationDisplacement, sourceIsIndirect, sourceDisplacement);
      }

      public static void XorPD(RegisterXMM destination, String sourceLabel, bool destinationIsIndirect = false, int? destinationDisplacement = null, bool sourceIsIndirect = false, int? sourceDisplacement = null)
      {
         DoDestinationSource<XorPD>(destination, sourceLabel, destinationIsIndirect, destinationDisplacement, sourceIsIndirect, sourceDisplacement);
      }
    }
  }
}
