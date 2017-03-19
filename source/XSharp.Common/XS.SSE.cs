using System;
using Cosmos.Assembler.x86.SSE;
using Cosmos.Assembler.x86.x87;

namespace XSharp.Common
{
  partial class XS
  {
    public static class SSE
    {
      public static void SSEInit()
      {
         XS.Comment("BEGIN - SSE Init");

         // CR4[bit 9]=1, CR4[bit 10]=1, CR0[bit 2]=0, CR0[bit 1]=1

         XS.Set(XSRegisters.EAX, XSRegisters.CR4);
         XS.Or(XSRegisters.EAX, 0x100);
         XS.Set(XSRegisters.CR4, XSRegisters.EAX);
         XS.Set(XSRegisters.EAX, XSRegisters.CR4);
         XS.Or(XSRegisters.EAX, 0x200);
         XS.Set(XSRegisters.CR4, XSRegisters.EAX);
         XS.Set(XSRegisters.EAX, XSRegisters.CR0);
         XS.And(XSRegisters.EAX, 0xfffffffd);
         XS.Set(XSRegisters.CR0, XSRegisters.EAX);
         XS.Set(XSRegisters.EAX, XSRegisters.CR0);

         XS.And(XSRegisters.EAX, 1);
         XS.Set(XSRegisters.CR0, XSRegisters.EAX);
         XS.Comment("END - SSE Init");
     }

      public static void AddSS(XSRegisters.RegisterXMM destination, XSRegisters.RegisterXMM source)
      {
        DoDestinationSource<AddSS>(destination, source);
      }

      public static void MulSS(XSRegisters.RegisterXMM destination, XSRegisters.RegisterXMM source)
      {
        DoDestinationSource<MulSS>(destination, source);
      }

      public static void SubSS(XSRegisters.RegisterXMM destination, XSRegisters.RegisterXMM source)
      {
        DoDestinationSource<SubSS>(destination, source);
      }

      public static void XorPS(XSRegisters.RegisterXMM destination, XSRegisters.RegisterXMM source)
      {
        DoDestinationSource<XorPS>(destination, source);
      }

      public static void CompareSS(XSRegisters.RegisterXMM destination, XSRegisters.RegisterXMM source, ComparePseudoOpcodes comparision)
      {
         new CompareSS()
         {
            DestinationReg = destination,
            SourceReg = source,
            pseudoOpcode = (byte) comparision
          };
      }

      public static void ConvertSI2SS(XSRegisters.RegisterXMM destination, XSRegisters.Register32 source, bool sourceIsIndirect = false)
      {
        new ConvertSI2SS()
        {
          DestinationReg = destination,
          SourceReg = source,
          SourceIsIndirect = sourceIsIndirect
        };
      }

      public static void MoveSS(XSRegisters.RegisterXMM destination, XSRegisters.Register32 source, bool sourceIsIndirect = false)
      {
        new MoveSS()
        {
          DestinationReg = destination,
          SourceReg = source,
          SourceIsIndirect = sourceIsIndirect
        };
      }

      public static void MoveSS(XSRegisters.Register32 destination, XSRegisters.RegisterXMM source, bool destinationIsIndirect = false)
      {
        new MoveSS()
        {
          DestinationReg = destination,
          DestinationIsIndirect = destinationIsIndirect,
          SourceReg = source
        };
      }

      public static void MoveSS(XSRegisters.RegisterXMM destination, String sourceLabel, bool destinationIsIndirect = false, int? destinationDisplacement = null, bool sourceIsIndirect = false, int? sourceDisplacement = null)
      {
          DoDestinationSource<MoveSS>(destination, sourceLabel, destinationIsIndirect, destinationDisplacement, sourceIsIndirect, sourceDisplacement);
      }

      public static void ConvertSS2SD(XSRegisters.RegisterXMM destination, XSRegisters.Register32 source, bool sourceIsIndirect = false)
      {
        new ConvertSS2SD()
        {
          DestinationReg = destination,
          SourceReg = source,
          SourceIsIndirect = sourceIsIndirect
        };
      }

      public static void ConvertSS2SIAndTruncate(XSRegisters.Register32 destination, XSRegisters.RegisterXMM source)
      {
        new ConvertSS2SIAndTruncate
        {
          DestinationReg = destination,
          SourceReg = source
        };
      }

      public static void DivPS(XSRegisters.RegisterXMM destination, XSRegisters.RegisterXMM source)
      {
        new DivPS
        {
          DestinationReg = destination,
          SourceReg = source
        };
      }

      public static void DivSS(XSRegisters.RegisterXMM destination, XSRegisters.RegisterXMM source)
      {
        new DivSS
        {
          DestinationReg = destination,
          SourceReg = source
        };
      }

      public static void FXSave(XSRegisters.Register32 destination, bool isIndirect)
      {
        new FXSave
        {
          DestinationReg = destination,
          DestinationIsIndirect = isIndirect
        };
      }

      public static void FXRestore(XSRegisters.Register32 destination, bool isIndirect)
      {
        new FXStore()
        {
          DestinationReg = destination,
          DestinationIsIndirect = isIndirect
        };
      }
    }
  }
}
