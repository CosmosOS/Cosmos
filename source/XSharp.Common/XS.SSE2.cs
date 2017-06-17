using System;
using Cosmos.Assembler.x86;
using Cosmos.Assembler.x86.SSE;

namespace XSharp.Common
{
  partial class XS
  {
    public static class SSE2
    {
      public static void AddSD(XSRegisters.RegisterXMM destination, XSRegisters.RegisterXMM source)
      {
        DoDestinationSource<AddSD>(destination, source);
      }

      public static void DivSD(XSRegisters.RegisterXMM destination, XSRegisters.RegisterXMM source)
      {
        new DivSD
        {
           DestinationReg = destination,
           SourceReg = source
        };
      }

      public static void MulSD(XSRegisters.RegisterXMM destination, XSRegisters.RegisterXMM source)
      {
          DoDestinationSource<MulSD>(destination, source);
      }

      public static void SubSD(XSRegisters.RegisterXMM destination, XSRegisters.RegisterXMM source)
      {
          DoDestinationSource<SubSD>(destination, source);
      }

      public static void CompareSD(XSRegisters.RegisterXMM destination, XSRegisters.RegisterXMM source, ComparePseudoOpcodes comparision)
      {
       new CompareSD()
       {
         DestinationReg = destination,
         SourceReg = source,
         pseudoOpcode = (byte)comparision
       };
      }

      public static void ConvertSD2SIAndTruncate(XSRegisters.Register32 destination, XSRegisters.RegisterXMM source)
      {
        new ConvertSD2SIAndTruncate
        {
          DestinationReg = destination,
          SourceReg = source
        };
      }

      public static void ConvertSD2SS(XSRegisters.RegisterXMM destination, XSRegisters.Register32 source, bool sourceIsIndirect = false)
      {
        new ConvertSD2SS()
        {
            DestinationReg = destination,
            SourceReg = source,
            SourceIsIndirect = sourceIsIndirect
        };
     }

     public static void ConvertSI2SD(XSRegisters.RegisterXMM destination, XSRegisters.Register32 source, bool sourceIsIndirect = false, int? sourceDisplacement = null, bool destinationIsIndirect = false, int? destinationDisplacement = null)
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

      public static void MoveD(string destination, XSRegisters.Register source, bool destinationIsIndirect = false, int? destinationDisplacement = null, bool sourceIsIndirect = false, int? sourceDisplacement = null)
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

      public static void MoveD(XSRegisters.Register destination, string sourceLabel, bool destinationIsIndirect = false, int? destinationDisplacement = null, bool sourceIsIndirect = false, int? sourceDisplacement = null)
      {
        DoDestinationSource<MoveD>(destination, sourceLabel, destinationIsIndirect, destinationDisplacement, sourceIsIndirect, sourceDisplacement);
      }

      public static void MoveD(XSRegisters.Register destination, uint value, bool destinationIsIndirect = false, int? destinationDisplacement = null, bool sourceIsIndirect = false, int? sourceDisplacement = null)
      {
        DoDestinationSource<MoveD>(destination, value, destinationIsIndirect, destinationDisplacement, sourceIsIndirect, sourceDisplacement);
      }

      public static void MoveD(XSRegisters.Register destination, XSRegisters.Register source, bool destinationIsIndirect = false, int? destinationDisplacement = null, bool sourceIsIndirect = false, int? sourceDisplacement = null)
      {
        DoDestinationSource<MoveD>(destination, source, destinationIsIndirect, destinationDisplacement, sourceIsIndirect, sourceDisplacement);
      }

      public static void MoveSD(XSRegisters.RegisterXMM destination, XSRegisters.Register32 source, bool sourceIsIndirect = false)
      {
        new MoveSD()
        {
          DestinationReg = destination,
          SourceReg = source,
          SourceIsIndirect = sourceIsIndirect
        };
      }

      public static void MoveSD(XSRegisters.Register32 destination, XSRegisters.RegisterXMM source, bool destinationIsIndirect = false)
      {
        new MoveSD()
        {
          DestinationReg = destination,
          DestinationIsIndirect = destinationIsIndirect,
          SourceReg = source
        };
      }

      public static void XorPD(XSRegisters.RegisterXMM destination, XSRegisters.RegisterXMM source, bool destinationIsIndirect = false, int? destinationDisplacement = null, bool sourceIsIndirect = false, int? sourceDisplacement = null)
      {
          DoDestinationSource<XorPD>(destination, source, destinationIsIndirect, destinationDisplacement, sourceIsIndirect, sourceDisplacement);
      }

      public static void XorPD(XSRegisters.RegisterXMM destination, String sourceLabel, bool destinationIsIndirect = false, int? destinationDisplacement = null, bool sourceIsIndirect = false, int? sourceDisplacement = null)
      {
         DoDestinationSource<XorPD>(destination, sourceLabel, destinationIsIndirect, destinationDisplacement, sourceIsIndirect, sourceDisplacement);
      }
    }
  }
}
