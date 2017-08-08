using System;
using Cosmos.Assembler.x86.x87;

namespace XSharp.Common
{
  public partial class XS
  {
    public static class FPU
    {
      public static void FloatCompareAndSet(XSRegisters.RegisterFPU register)
      {
        new FloatCompareAndSet
        {
          DestinationReg = register.RegEnum
        };
      }

      public static void FloatStoreAndPop(XSRegisters.Register32 register, bool isIndirect = false, int? displacement = null, XSRegisters.RegisterSize? size = null)
      {
        if (displacement != null)
        {
          isIndirect = true;
          if (displacement == 0)
          {
            displacement = null;
          }
        }

        if (size == null)
        {
          if (isIndirect)
          {
            throw new InvalidOperationException("No size specified!");
          }
          size = register.Size;
        }

        new FloatStoreAndPop
        {
          DestinationReg = register.RegEnum,
          DestinationIsIndirect = isIndirect,
          DestinationDisplacement = displacement,
          Size = (byte)size
        };
      }

      public static void FloatStoreAndPop(XSRegisters.RegisterFPU register)
      {
        new FloatStoreAndPop
        {
          DestinationReg = register.RegEnum
        };
      }

      public static void FloatLoad(XSRegisters.Register32 register, bool destinationIsIndirect = false, int? displacement = null, XSRegisters.RegisterSize? size = null)
      {
        Do<FloatLoad>(register, isIndirect: destinationIsIndirect, displacement: displacement, size: size);
      }

      public static void FloatAbs()
      {
        new FloatABS();
      }

      public static void FloatInit()
      {
        new FloatInit();
      }

      public static void FloatNegate()
      {
        new FloatNegate();
      }

      public static void FloatAdd(XSRegisters.Register32 destination, bool isIndirect = false, XSRegisters.RegisterSize? size = null)
      {
        if (size == null)
        {
          if (isIndirect)
          {
            throw new InvalidOperationException("No size specified!");
          }
          size = destination.Size;
        }

        new FloatAdd
        {
          DestinationReg = destination,
          DestinationIsIndirect = isIndirect,
          Size = (byte)size.Value
        };
      }

      public static void IntLoad(XSRegisters.Register32 destination, bool isIndirect = false, XSRegisters.RegisterSize? size = null)
      {
        if (size == null)
        {
          if (isIndirect)
          {
            throw new InvalidOperationException("No size specified!");
          }
          size = destination.Size;
        }

        new IntLoad
        {
          DestinationReg = destination,
          DestinationIsIndirect = isIndirect,
          Size = (byte)size.Value
        };
      }

      public static void IntStoreWithTruncate(XSRegisters.Register32 destination, bool isIndirect = false, XSRegisters.RegisterSize? size = null)
      {
        if (size == null)
        {
          if (isIndirect)
          {
            throw new InvalidOperationException("No size specified!");
          }
          size = destination.Size;
        }

        new IntStoreWithTrunc
        {
          DestinationReg = destination,
          DestinationIsIndirect = isIndirect,
          Size = (byte)size.Value
        };
      }
    }
  }
}
