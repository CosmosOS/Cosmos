using System;
using Cosmos.Assembler.x86.SSE;
using Cosmos.Assembler.x86.x87;
using static XSharp.Compiler.XSRegisters;

namespace XSharp.Compiler
{
  partial class XS
  {
    public static class SSE
    {
      public static void SSEInit()
      {
         XS.Comment("BEGIN - SSE Init");

         // CR4[bit 9]=1, CR4[bit 10]=1, CR0[bit 2]=0, CR0[bit 1]=1

         XS.Set(EAX, CR4);
         XS.Or(EAX, 0x100);
         XS.Set(CR4, EAX);
         XS.Set(EAX, CR4);
         XS.Or(EAX, 0x200);
         XS.Set(CR4, EAX);
         XS.Set(EAX, CR0);
         XS.And(EAX, 0xfffffffd);
         XS.Set(CR0, EAX);
         XS.Set(EAX, CR0);

         XS.And(EAX, 1);
         XS.Set(CR0, EAX);
         XS.Comment("END - SSE Init");
     }

      public static void AddSS(RegisterXMM destination, RegisterXMM source)
      {
        DoDestinationSource<AddSS>(destination, source);
      }

      public static void MulSS(RegisterXMM destination, RegisterXMM source)
      {
        DoDestinationSource<MulSS>(destination, source);
      }

      public static void SubSS(RegisterXMM destination, RegisterXMM source)
      {
        DoDestinationSource<SubSS>(destination, source);
      }

      public static void XorPS(RegisterXMM destination, RegisterXMM source)
      {
        DoDestinationSource<XorPS>(destination, source);
      }

      public static void CompareSS(RegisterXMM destination, RegisterXMM source, ComparePseudoOpcodes comparision)
      {
         new CompareSS()
         {
            DestinationReg = destination,
            SourceReg = source,
            pseudoOpcode = (byte) comparision
          };
      }

      public static void ConvertSI2SS(RegisterXMM destination, Register32 source, bool sourceIsIndirect = false)
      {
        new ConvertSI2SS()
        {
          DestinationReg = destination,
          SourceReg = source,
          SourceIsIndirect = sourceIsIndirect
        };
      }

      public static void MoveSS(RegisterXMM destination, Register32 source, bool sourceIsIndirect = false)
      {
        new MoveSS()
        {
          DestinationReg = destination,
          SourceReg = source,
          SourceIsIndirect = sourceIsIndirect
        };
      }

      public static void MoveSS(Register32 destination, RegisterXMM source, bool destinationIsIndirect = false)
      {
        new MoveSS()
        {
          DestinationReg = destination,
          DestinationIsIndirect = destinationIsIndirect,
          SourceReg = source
        };
      }

      public static void MoveSS(RegisterXMM destination, String sourceLabel, bool destinationIsIndirect = false, int? destinationDisplacement = null, bool sourceIsIndirect = false, int? sourceDisplacement = null)
      {
          DoDestinationSource<MoveSS>(destination, sourceLabel, destinationIsIndirect, destinationDisplacement, sourceIsIndirect, sourceDisplacement);
      }

      public static void ConvertSS2SD(RegisterXMM destination, Register32 source, bool sourceIsIndirect = false)
      {
        new ConvertSS2SD()
        {
          DestinationReg = destination,
          SourceReg = source,
          SourceIsIndirect = sourceIsIndirect
        };
      }

      public static void ConvertSS2SIAndTruncate(Register32 destination, RegisterXMM source)
      {
        new ConvertSS2SIAndTruncate
        {
          DestinationReg = destination,
          SourceReg = source
        };
      }

      public static void DivPS(RegisterXMM destination, RegisterXMM source)
      {
        new DivPS
        {
          DestinationReg = destination,
          SourceReg = source
        };
      }

      public static void DivSS(RegisterXMM destination, RegisterXMM source)
      {
        new DivSS
        {
          DestinationReg = destination,
          SourceReg = source
        };
      }

      public static void FXSave(Register32 destination, bool isIndirect)
      {
        new FXSave
        {
          DestinationReg = destination,
          DestinationIsIndirect = isIndirect
        };
      }

      public static void FXRestore(Register32 destination, bool isIndirect)
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
