using System;
using System.Diagnostics.CodeAnalysis;
using Cosmos.Assembler;
using Cosmos.Assembler.x86;
using Cosmos.Assembler.x86.x87;
using static XSharp.Compiler.XSRegisters;

namespace XSharp.Compiler
{
  [SuppressMessage("ReSharper", "ObjectCreationAsStatement")]
  public static partial class XS
  {
    public static void Label(string labelName)
    {
      new Label(labelName);
    }

    public static void Return()
    {
      new Return();
    }

    public static void InterruptReturn()
    {
      new IRET();
    }

    #region InstructionWithDestinationAndSourceAndSize

    private static void Do<T>(string destination, Register source, bool destinationIsIndirect = false, int? destinationDisplacement = null, bool sourceIsIndirect = false, int? sourceDisplacement = null, RegisterSize? size = null)
      where T : InstructionWithDestinationAndSourceAndSize, new()
    {
      if (destinationDisplacement != null)
      {
        destinationIsIndirect = true;
        if (destinationDisplacement == 0)
        {
          destinationDisplacement = null;
        }
      }
      if (sourceDisplacement != null)
      {
        sourceIsIndirect = true;
        if (sourceDisplacement == 0)
        {
          sourceDisplacement = null;
        }
      }
      if (destinationIsIndirect && sourceIsIndirect)
      {
        throw new Exception("Both destination and source cannot be indirect!");
      }

      new T
      {
        Size = (byte)source.Size,
        DestinationRef = ElementReference.New(destination),
        DestinationIsIndirect = destinationIsIndirect,
        DestinationDisplacement = destinationDisplacement,
        SourceReg = source.RegEnum,
        SourceIsIndirect = sourceIsIndirect,
        SourceDisplacement = sourceDisplacement
      };
    }

    private static void Do<T>(string destination, UInt32 value, bool destinationIsIndirect = false, int? destinationDisplacement = null, bool sourceIsIndirect = false, int? sourceDisplacement = null, RegisterSize size = RegisterSize.Int32)
      where T : InstructionWithDestinationAndSourceAndSize, new()
    {
      if (destinationDisplacement != null)
      {
        destinationIsIndirect = true;
        if (destinationDisplacement == 0)
        {
          destinationDisplacement = null;
        }
      }
      if (sourceDisplacement != null)
      {
        sourceIsIndirect = true;
        if (sourceDisplacement == 0)
        {
          sourceDisplacement = null;
        }
      }
      if (destinationIsIndirect && sourceIsIndirect)
      {
        throw new Exception("Both destination and source cannot be indirect!");
      }

      new T
      {
        Size = (byte)size,
        DestinationRef = ElementReference.New(destination),
        DestinationIsIndirect = destinationIsIndirect,
        DestinationDisplacement = destinationDisplacement,
        SourceValue = value,
        SourceIsIndirect = sourceIsIndirect,
        SourceDisplacement = sourceDisplacement,
      };
    }

    private static void Do<T>(string destination, string source, bool destinationIsIndirect = false, int? destinationDisplacement = null, bool sourceIsIndirect = false, int? sourceDisplacement = null, RegisterSize size = RegisterSize.Int32)
      where T : InstructionWithDestinationAndSourceAndSize, new()
    {
      if (destinationDisplacement != null)
      {
        destinationIsIndirect = true;
        if (destinationDisplacement == 0)
        {
          destinationDisplacement = null;
        }
      }
      if (sourceDisplacement != null)
      {
        sourceIsIndirect = true;
        if (sourceDisplacement == 0)
        {
          sourceDisplacement = null;
        }
      }
      if (destinationIsIndirect && sourceIsIndirect)
      {
        throw new Exception("Both destination and source cannot be indirect!");
      }

      new T
      {
        Size = (byte)size,
        DestinationRef = ElementReference.New(destination),
        DestinationIsIndirect = destinationIsIndirect,
        DestinationDisplacement = destinationDisplacement,
        SourceRef = ElementReference.New(source),
        SourceIsIndirect = sourceIsIndirect,
        SourceDisplacement = sourceDisplacement,
      };
    }

    private static void Do<T>(Register destination, string sourceLabel, bool destinationIsIndirect = false, int? destinationDisplacement = null, bool sourceIsIndirect = false, int? sourceDisplacement = null, RegisterSize? size = null)
      where T: InstructionWithDestinationAndSourceAndSize, new()
    {
      if (destinationDisplacement != null)
      {
        destinationIsIndirect = true;
        if (destinationDisplacement == 0)
        {
          destinationDisplacement = null;
        }
      }
      if (sourceDisplacement != null)
      {
        sourceIsIndirect = true;
        if (sourceDisplacement == 0)
        {
          sourceDisplacement = null;
        }
      }

      if (size == null)
      {
        if (destinationIsIndirect)
        {
          throw new Exception("No size specified!");
        }
        size = destination.Size;
      }

      new T
      {
        Size = (byte)size.Value,
        DestinationReg = destination.RegEnum,
        DestinationIsIndirect = destinationIsIndirect,
        DestinationDisplacement = destinationDisplacement,
        SourceRef = ElementReference.New(sourceLabel),
        SourceIsIndirect = sourceIsIndirect,
        SourceDisplacement = sourceDisplacement
      };
    }

    private static void Do<T>(Register destination, uint value, bool destinationIsIndirect = false, int? destinationDisplacement = null, bool sourceIsIndirect = false, int? sourceDisplacement = null, RegisterSize? size = null)
      where T: InstructionWithDestinationAndSourceAndSize, new()
    {
      if (destinationDisplacement != null)
      {
        destinationIsIndirect = true;
        if (destinationDisplacement == 0)
        {
          destinationDisplacement = null;
        }
      }

      if (sourceDisplacement != null)
      {
        sourceIsIndirect = true;
        if (sourceDisplacement == 0)
        {
          sourceDisplacement = null;
        }
      }

      if (size == null)
      {
        if (destinationIsIndirect)
        {
          throw new Exception("No size specified!");
        }

        size = destination.Size;
      }

      new T
      {
        Size = (byte)size,
        DestinationReg = destination.RegEnum,
        DestinationIsIndirect = destinationIsIndirect,
        DestinationDisplacement = destinationDisplacement,
        SourceValue = value,
        SourceIsIndirect = sourceIsIndirect,
        SourceDisplacement = sourceDisplacement,
      };
    }

    private static void Do<T>(Register destination,
                              Register source,
                              bool destinationIsIndirect = false,
                              int? destinationDisplacement = null,
                              bool sourceIsIndirect = false,
                              int? sourceDisplacement = null)
      where T : InstructionWithDestinationAndSourceAndSize, new()
    {
      if (destinationDisplacement != null)
      {
        destinationIsIndirect = true;
        if (destinationDisplacement == 0)
        {
          destinationDisplacement = null;
        }
      }
      if (sourceDisplacement != null)
      {
        sourceIsIndirect = true;
        if (sourceDisplacement == 0)
        {
          sourceDisplacement = null;
        }
      }
      if (!(destinationIsIndirect || sourceIsIndirect)
          && destination.Size != source.Size)
      {
        throw new Exception("Register sizes must match!");
      }
      if (destinationIsIndirect && sourceIsIndirect)
      {
        throw new Exception("Both destination and source cannot be indirect!");
      }
      RegisterSize xSize;
      if (!destinationIsIndirect)
      {
        xSize = destination.Size;
      }
      else
      {
        xSize = source.Size;
      }

      new T
      {
        Size = (byte)xSize,
        DestinationReg = destination.RegEnum,
        DestinationIsIndirect = destinationIsIndirect,
        DestinationDisplacement = destinationDisplacement,
        SourceIsIndirect = sourceIsIndirect,
        SourceDisplacement = sourceDisplacement,
        SourceReg = source.RegEnum
      };
    }

    #endregion InstructionWithDestinationAndSourceAndSize

    #region InstructionWithDestinationAndSize
    private static void Do<T>(uint destinationValue, RegisterSize size = RegisterSize.Int32)
      where T: InstructionWithDestinationAndSize, new()
    {
      new T
      {
        DestinationValue = destinationValue,
        Size = (byte)size
      };
    }

    private static void Do<T>(Register register)
      where T: InstructionWithDestinationAndSize, new()
    {
      new T
      {
        DestinationReg = register.RegEnum
      };
    }

    private static void Do<T>(string label, bool isIndirect = false, RegisterSize size = RegisterSize.Int32)
      where T: InstructionWithDestinationAndSize, new()
    {
      new T
      {
        DestinationRef = ElementReference.New(label),
        DestinationIsIndirect = isIndirect,
        Size = (byte)size
      };
    }

    #endregion InstructionWithDestinationAndSize

    #region Mov
    public static void Set(string destination, Register source, bool destinationIsIndirect = false, int? destinationDisplacement = null, bool sourceIsIndirect = false, int? sourceDisplacement = null, RegisterSize? size = null)
    {
      Do<Mov>(destination, source, destinationIsIndirect, destinationDisplacement, sourceIsIndirect, sourceDisplacement, size);
    }

    public static void Set(string destination, UInt32 value, bool destinationIsIndirect = false, int? destinationDisplacement = null, bool sourceIsIndirect = false, int? sourceDisplacement = null, RegisterSize size = RegisterSize.Int32)
    {
      Do<Mov>(destination, value, destinationIsIndirect, destinationDisplacement, sourceIsIndirect, sourceDisplacement, size);
    }

    public static void Set(string destination, string source, bool destinationIsIndirect = false, int? destinationDisplacement = null, bool sourceIsIndirect = false, int? sourceDisplacement = null, RegisterSize size = RegisterSize.Int32)
    {
      Do<Mov>(destination, source, destinationIsIndirect, destinationDisplacement, sourceIsIndirect, sourceDisplacement, size);
    }

    public static void Set(Register destination, string sourceLabel, bool destinationIsIndirect = false, int? destinationDisplacement = null, bool sourceIsIndirect = false, int? sourceDisplacement = null, RegisterSize? size = null)
    {
      Do<Mov>(destination, sourceLabel, destinationIsIndirect, destinationDisplacement, sourceIsIndirect, sourceDisplacement, size);
    }

    public static void Set(Register destination, uint value, bool destinationIsIndirect = false, int? destinationDisplacement = null, bool sourceIsIndirect = false, int? sourceDisplacement = null, RegisterSize? size = null)
    {
      Do<Mov>(destination, value, destinationIsIndirect, destinationDisplacement, sourceIsIndirect, sourceDisplacement, size);
    }

    public static void Set(Register destination, Register source, bool destinationIsIndirect = false, int? destinationDisplacement = null, bool sourceIsIndirect = false, int? sourceDisplacement = null)
    {
      Do<Mov>(destination, source, destinationIsIndirect, destinationDisplacement, sourceIsIndirect, sourceDisplacement);
    }

    public static void SetByte(uint address, byte value)
    {
      new Mov { DestinationValue = address, DestinationIsIndirect = true, SourceValue = value };
    }

    #endregion Mov

    public static void Jump(ConditionalTestEnum condition, string label)
    {
      new ConditionalJump { Condition = condition, DestinationLabel = label };
    }

    public static void Jump(string label)
    {
      new Jump { DestinationLabel = label };
    }

    public static void Comment(string comment)
    {
      new Comment(comment);
    }

    public static void Call(string target)
    {
      new Call { DestinationLabel=target };
    }

    public static void Const(string name, string value)
    {
      new LiteralAssemblerCode(name + " equ " + value);
    }

    public static void DataMember(string name, uint value = 0)
    {
      Assembler.CurrentInstance.DataMembers.Add(new DataMember(name, value));
    }

    public static void DataMember(string name, string value)
    {
      Assembler.CurrentInstance.DataMembers.Add(new DataMember(name, value));
    }

    public static void DataMember(string name, uint elementCount, string size, string value)
    {
      new LiteralAssemblerCode(name + ": TIMES " + elementCount + " " + size + " " + value);
    }

    public static void RotateRight(Register register, uint bitCount)
    {
      Do<RotateRight>(register, bitCount);
    }

    public static void RotateLeft(Register register, uint bitCount)
    {
      Do<RotateLeft>(register, bitCount);
    }

    public static void ShiftRight(Register register, uint bitCount)
    {
      Do<ShiftRight>(register, bitCount);
    }

    public static void ShiftLeft(Register register, uint bitCount)
    {
      Do<ShiftLeft>(register, bitCount);
    }

    public static void PushAllGeneralRegisters()
    {
      new Pushad();
    }

    public static void PopAllGeneralRegisters()
    {
      new Popad();
    }

    public static void WriteToPortDX(Register value)
    {
      new OutToDX()
      {
        DestinationReg = value.RegEnum
      };
    }

    public static void ReadFromPortDX(Register value)
    {
      new InFromDX
      {
        DestinationReg = value.RegEnum
      };
    }

    public static void Push(Register value)
    {
      Do<Push>(value);
    }

    public static void Push(uint value, RegisterSize size)
    {
      Do<Push>(value, size);
    }

    public static void Push(string label, bool isIndirect = false, RegisterSize size = RegisterSize.Int32)
    {
      Do<Push>(label, isIndirect, size);
    }

    public static void Pop(Register value)
    {
      Do<Pop>(value);
    }

    public static void Increment(Register value)
    {
      Do<INC>(value);
    }

    public static void Decrement(Register value)
    {
      Do<Dec>(value);
    }

    public static void Add(Register register, uint valueToAdd)
    {
      Do<Add>(register, valueToAdd);
    }

    public static void Add(Register register, Register valueToAdd)
    {
      Do<Add>(register, valueToAdd);
    }

    public static void Sub(Register register, uint valueToAdd)
    {
      Do<Sub>(register, valueToAdd);
    }

    public static void Sub(Register register, Register valueToAdd)
    {
      Do<Sub>(register, valueToAdd);
    }

    public static void And(Register register, uint value)
    {
      Do<And>(register, value);
    }

    public static void And(Register register, Register value)
    {
      Do<And>(register, value);
    }

    public static void Or(Register register, uint value)
    {
      Do<Or>(register, value);
    }

    public static void Or(Register register, Register value)
    {
      Do<Or>(register, value);
    }

    public static void Xor(Register register, uint value)
    {
      Do<Xor>(register, value);
    }

    public static void Xor(Register register, Register value)
    {
      Do<Xor>(register, value);
    }

    public static void IntegerMultiply(Register register, uint valueToAdd)
    {
      Do<Imul>(register, valueToAdd);
    }

    public static void IntegerMultiply(Register register, Register registerToAdd)
    {
      Do<Imul>(register, registerToAdd);
    }

    #region Compare

    public static void Compare(string destination, Register source, bool destinationIsIndirect = false, int? destinationDisplacement = null, bool sourceIsIndirect = false, int? sourceDisplacement = null, RegisterSize? size = null)
    {
      Do<Compare>(destination, source, destinationIsIndirect, destinationDisplacement, sourceIsIndirect, sourceDisplacement, size);
    }

    public static void Compare(string destination, UInt32 value, bool destinationIsIndirect = false, int? destinationDisplacement = null, bool sourceIsIndirect = false, int? sourceDisplacement = null, RegisterSize size = RegisterSize.Int32)
    {
      Do<Compare>(destination, value, destinationIsIndirect, destinationDisplacement, sourceIsIndirect, sourceDisplacement, size);
    }

    public static void Compare(string destination, string source, bool destinationIsIndirect = false, int? destinationDisplacement = null, bool sourceIsIndirect = false, int? sourceDisplacement = null, RegisterSize size = RegisterSize.Int32)
    {
      Do<Compare>(destination, source, destinationIsIndirect, destinationDisplacement, sourceIsIndirect, sourceDisplacement, size);
    }

    public static void Compare(Register destination, string sourceLabel, bool destinationIsIndirect = false, int? destinationDisplacement = null, bool sourceIsIndirect = false, int? sourceDisplacement = null, RegisterSize? size = null)
    {
      Do<Compare>(destination, sourceLabel, destinationIsIndirect, destinationDisplacement, sourceIsIndirect, sourceDisplacement, size);
    }

    public static void Compare(Register destination, uint value, bool destinationIsIndirect = false, int? destinationDisplacement = null, bool sourceIsIndirect = false, int? sourceDisplacement = null, RegisterSize? size = null)
    {
      Do<Compare>(destination, value, destinationIsIndirect, destinationDisplacement, sourceIsIndirect, sourceDisplacement, size);
    }

    public static void Compare(Register destination, Register source, bool destinationIsIndirect = false, int? destinationDisplacement = null, bool sourceIsIndirect = false, int? sourceDisplacement = null)
    {
      Do<Compare>(destination, source, destinationIsIndirect, destinationDisplacement, sourceIsIndirect, sourceDisplacement);
    }

    #endregion Compare

    public static void LiteralCode(string code)
    {
      new LiteralAssemblerCode(code);
    }

    public static void Test(Register destination, uint source)
    {
      new Test
      {
        DestinationReg = destination.RegEnum,
        SourceValue = source
      };
    }

    public static void Test(Register destination, string sourceRef, bool sourceIsIndirect = false)
    {
      new Test
      {
        DestinationReg = destination.RegEnum,
        SourceRef = ElementReference.New(sourceRef),
        SourceIsIndirect = sourceIsIndirect
      };
    }
  }
}
