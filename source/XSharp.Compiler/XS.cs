using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection.Emit;
using Cosmos.Assembler;
using Cosmos.Assembler.x86;
using Cosmos.Assembler.x86.x87;
using static XSharp.Compiler.XSRegisters;
using Label = Cosmos.Assembler.Label;

//TODO: Fix indentaion and formatting in this file: ideal would be 4space-indent

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

      if (size == null)
      {
        if (destinationIsIndirect)
        {
          size = RegisterSize.Int32;
        }
        else
        {
          size = destination.Size;
        }
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

    private static void Do<T>(Register destination, Register source, bool destinationIsIndirect = false, int? destinationDisplacement = null, bool sourceIsIndirect = false, int? sourceDisplacement = null, bool skipSizeCheck = false, RegisterSize? size = null)
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
      if (!skipSizeCheck && !(destinationIsIndirect || sourceIsIndirect) && destination.Size != source.Size)
      {
        throw new Exception("Register sizes must match!");
      }
      if (destinationIsIndirect && sourceIsIndirect)
      {
        throw new Exception("Both destination and source cannot be indirect!");
      }
      if (size == null)
      {
        if (!destinationIsIndirect)
        {
          size = destination.Size;
        }
        else
        {
          size = source.Size;
        }
      }

      new T
      {
        Size = (byte)size,
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
    private static void Do<T>(uint destinationValue, bool isIndirect = false, int? displacement = null, RegisterSize size = RegisterSize.Int32)
      where T : InstructionWithDestinationAndSize, new()
    {
      if (displacement != null)
      {
        isIndirect = true;
        if (displacement == 0)
        {
          displacement = null;
        }
      }

      new T
      {
        DestinationValue = destinationValue,
        DestinationIsIndirect = isIndirect,
        DestinationDisplacement = displacement,
        Size = (byte)size
      };
    }

    private static void Do<T>(Register register, bool isIndirect = false, int? displacement = null, RegisterSize? size = null)
      where T : InstructionWithDestinationAndSize, new()
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
      new T
      {
        DestinationReg = register.RegEnum,
        DestinationIsIndirect = isIndirect,
        DestinationDisplacement = displacement,
        Size = (byte)size.Value
      };
    }

    private static void Do<T>(string label, bool isIndirect = false, int? displacement = null, RegisterSize size = RegisterSize.Int32)
      where T : InstructionWithDestinationAndSize, new()
    {
      if (displacement != null)
      {
        isIndirect = true;
        if (displacement == 0)
        {
          displacement = null;
        }
      }

      new T
      {
        DestinationRef = ElementReference.New(label),
        DestinationIsIndirect = isIndirect,
        DestinationDisplacement = displacement,
        Size = (byte)size
      };
    }

    #endregion InstructionWithDestinationAndSize

    #region InstructionWithDestinationAndSource

    private static void DoDestinationSource<T>(string destination, Register source, bool destinationIsIndirect = false, int? destinationDisplacement = null, bool sourceIsIndirect = false, int? sourceDisplacement = null)
      where T : InstructionWithDestinationAndSource, new()
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
        DestinationRef = ElementReference.New(destination),
        DestinationIsIndirect = destinationIsIndirect,
        DestinationDisplacement = destinationDisplacement,
        SourceReg = source.RegEnum,
        SourceIsIndirect = sourceIsIndirect,
        SourceDisplacement = sourceDisplacement
      };
    }

    private static void DoDestinationSource<T>(string destination, UInt32 value, bool destinationIsIndirect = false, int? destinationDisplacement = null, bool sourceIsIndirect = false, int? sourceDisplacement = null)
      where T : InstructionWithDestinationAndSource, new()
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
        DestinationRef = ElementReference.New(destination),
        DestinationIsIndirect = destinationIsIndirect,
        DestinationDisplacement = destinationDisplacement,
        SourceValue = value,
        SourceIsIndirect = sourceIsIndirect,
        SourceDisplacement = sourceDisplacement,
      };
    }

    private static void DoDestinationSource<T>(string destination, string source, bool destinationIsIndirect = false, int? destinationDisplacement = null, bool sourceIsIndirect = false, int? sourceDisplacement = null)
      where T : InstructionWithDestinationAndSource, new()
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
        DestinationRef = ElementReference.New(destination),
        DestinationIsIndirect = destinationIsIndirect,
        DestinationDisplacement = destinationDisplacement,
        SourceRef = ElementReference.New(source),
        SourceIsIndirect = sourceIsIndirect,
        SourceDisplacement = sourceDisplacement,
      };
    }

    private static void DoDestinationSource<T>(Register destination, string sourceLabel, bool destinationIsIndirect = false, int? destinationDisplacement = null, bool sourceIsIndirect = false, int? sourceDisplacement = null)
      where T : InstructionWithDestinationAndSource, new()
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

      new T
      {
        DestinationReg = destination.RegEnum,
        DestinationIsIndirect = destinationIsIndirect,
        DestinationDisplacement = destinationDisplacement,
        SourceRef = ElementReference.New(sourceLabel),
        SourceIsIndirect = sourceIsIndirect,
        SourceDisplacement = sourceDisplacement
      };
    }

    private static void DoDestinationSource<T>(Register destination, uint value, bool destinationIsIndirect = false, int? destinationDisplacement = null, bool sourceIsIndirect = false, int? sourceDisplacement = null)
      where T : InstructionWithDestinationAndSource, new()
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

      new T
      {
        DestinationReg = destination.RegEnum,
        DestinationIsIndirect = destinationIsIndirect,
        DestinationDisplacement = destinationDisplacement,
        SourceValue = value,
        SourceIsIndirect = sourceIsIndirect,
        SourceDisplacement = sourceDisplacement,
      };
    }

    private static void DoDestinationSource<T>(Register destination,
                                               Register source,
                                               bool destinationIsIndirect = false,
                                               int? destinationDisplacement = null,
                                               bool sourceIsIndirect = false,
                                               int? sourceDisplacement = null)
      where T : InstructionWithDestinationAndSource, new()
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
        DestinationReg = destination.RegEnum,
        DestinationIsIndirect = destinationIsIndirect,
        DestinationDisplacement = destinationDisplacement,
        SourceIsIndirect = sourceIsIndirect,
        SourceDisplacement = sourceDisplacement,
        SourceReg = source.RegEnum
      };
    }

    #endregion InstructionWithDestinationAndSource

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

    public static void Set(Register destination, Register source, bool destinationIsIndirect = false, int? destinationDisplacement = null, bool sourceIsIndirect = false, int? sourceDisplacement = null, RegisterSize? size = null)
    {
      Do<Mov>(destination, source, destinationIsIndirect, destinationDisplacement, sourceIsIndirect, sourceDisplacement, size: size);
    }

    public static void SetByte(uint address, byte value)
    {
      new Mov { DestinationValue = address, DestinationIsIndirect = true, SourceValue = value };
    }

    #endregion Mov

    #region Lea

    public static void Lea(string destination, Register source, bool destinationIsIndirect = false, int? destinationDisplacement = null, bool sourceIsIndirect = false, int? sourceDisplacement = null, RegisterSize? size = null)
    {
      Do<Lea>(destination, source, destinationIsIndirect, destinationDisplacement, sourceIsIndirect, sourceDisplacement, size);
    }

    public static void Lea(string destination, UInt32 value, bool destinationIsIndirect = false, int? destinationDisplacement = null, bool sourceIsIndirect = false, int? sourceDisplacement = null, RegisterSize size = RegisterSize.Int32)
    {
      Do<Lea>(destination, value, destinationIsIndirect, destinationDisplacement, sourceIsIndirect, sourceDisplacement, size);
    }

    public static void Lea(string destination, string source, bool destinationIsIndirect = false, int? destinationDisplacement = null, bool sourceIsIndirect = false, int? sourceDisplacement = null, RegisterSize size = RegisterSize.Int32)
    {
      Do<Lea>(destination, source, destinationIsIndirect, destinationDisplacement, sourceIsIndirect, sourceDisplacement, size);
    }

    public static void Lea(Register destination, string sourceLabel, bool destinationIsIndirect = false, int? destinationDisplacement = null, bool sourceIsIndirect = false, int? sourceDisplacement = null, RegisterSize? size = null)
    {
      Do<Lea>(destination, sourceLabel, destinationIsIndirect, destinationDisplacement, sourceIsIndirect, sourceDisplacement, size);
    }

    public static void Lea(Register destination, uint value, bool destinationIsIndirect = false, int? destinationDisplacement = null, bool sourceIsIndirect = false, int? sourceDisplacement = null, RegisterSize? size = null)
    {
      Do<Lea>(destination, value, destinationIsIndirect, destinationDisplacement, sourceIsIndirect, sourceDisplacement, size);
    }

    public static void Lea(Register destination, Register source, bool destinationIsIndirect = false, int? destinationDisplacement = null, bool sourceIsIndirect = false, int? sourceDisplacement = null, RegisterSize? size = null)
    {
      Do<Lea>(destination, source, destinationIsIndirect, destinationDisplacement, sourceIsIndirect, sourceDisplacement, size: size);
    }

    #endregion

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
      new Call { DestinationLabel = target };
    }

    public static void Call(Register32 register)
    {
      new Call { DestinationReg = register.RegEnum };
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

    public static void DataMemberBytes(string name, byte[] value)
    {
      Assembler.CurrentInstance.DataMembers.Add(new DataMember(name, value));
    }

    public static void DataMember(string name, uint elementCount, string size, string value)
    {
      Assembler.CurrentInstance.DataMembers.Add(new DataMember(name, size, value));
    }

    public static void RotateRight(Register register, uint bitCount)
    {
      Do<RotateRight>(register, bitCount);
    }

    public static void RotateLeft(Register register, uint bitCount)
    {
      Do<RotateLeft>(register, bitCount);
    }

    public static void ShiftRight(Register destination, byte bitCount)
    {
      Do<ShiftRight>(destination, bitCount);
    }

    public static void ShiftRight(Register destination, Register8 source, bool destinationIsIndirect = false, int? destinationDisplacement = null, RegisterSize? size = null)
    {
      if (source != CL)
      {
        throw new InvalidOperationException();
      }
      Do<ShiftRight>(destination, source, skipSizeCheck: true, destinationIsIndirect: destinationIsIndirect, destinationDisplacement: destinationDisplacement, size: size);
    }

    public static void ShiftLeft(Register destination, byte bitCount)
    {
      Do<ShiftLeft>(destination, bitCount);
    }

    public static void ShiftLeft(Register destination, Register8 bitCount, bool destinationIsIndirect = false, RegisterSize size = RegisterSize.Int32)
    {
      if (bitCount != CL)
      {
        throw new InvalidOperationException();
      }
      Do<ShiftLeft>(destination, bitCount, destinationIsIndirect: destinationIsIndirect, size: size);
    }

    public static void ShiftRightArithmetic(Register destination, byte bitCount)
    {
      Do<ShiftRightArithmetic>(destination, bitCount);
    }

    public static void ShiftRightArithmetic(Register destination, Register8 source, bool destinationIsIndirect = false, int? destinationDisplacement = null, RegisterSize? size = null)
    {
      if (source != CL)
      {
        throw new InvalidOperationException();
      }

      Do<ShiftRightArithmetic>(destination, source, skipSizeCheck: true, destinationIsIndirect: destinationIsIndirect, destinationDisplacement: destinationDisplacement, size: size);
    }

    public static void ShiftLeftArithmetic(Register destination, byte bitCount)
    {
      Do<ShiftLeftArithmetic>(destination, bitCount);
    }

    public static void ShiftLeftArithmetic(Register destination, Register8 bitCount, bool destinationIsIndirect = false, RegisterSize size = RegisterSize.Int32)
    {
      if (bitCount != CL)
      {
        throw new InvalidOperationException();
      }
      Do<ShiftLeftArithmetic>(destination, bitCount, skipSizeCheck: true, destinationIsIndirect: destinationIsIndirect, size: size);
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

    public static void Push(uint destinationValue, bool isIndirect = false, int? displacement = null, RegisterSize size = RegisterSize.Int32)
    {
      Do<Push>(destinationValue, isIndirect, displacement, size);
    }

    public static void Push(Register register, bool isIndirect = false, int? displacement = null, RegisterSize size = RegisterSize.Int32)
    {
      Do<Push>(register, isIndirect, displacement, size);
    }

    public static void Push(string label, bool isIndirect = false, int? displacement = null, RegisterSize size = RegisterSize.Int32)
    {
      Do<Push>(label, isIndirect, displacement, size);
    }

    public static void Pushfd()
    {
      new Pushfd();
    }

    public static void Popfd()
    {
      new Popfd();
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

    public static void Add(Register register, uint valueToAdd, bool destinationIsIndirect = false)
    {
      Do<Add>(register, valueToAdd, destinationIsIndirect: destinationIsIndirect);
    }

    public static void Add(Register register, Register valueToAdd, bool destinationIsIndirect = false)
    {
      Do<Add>(register, valueToAdd, destinationIsIndirect: destinationIsIndirect);
    }

    public static void Sub(Register register, uint valueToAdd)
    {
      Do<Sub>(register, valueToAdd);
    }

    public static void Sub(Register register, Register valueToAdd, bool destinationIsIndirect = false)
    {
      Do<Sub>(register, valueToAdd, destinationIsIndirect: destinationIsIndirect);
    }

    public static void SubWithCarry(Register register, uint valueToAdd)
    {
      Do<SubWithCarry>(register, valueToAdd);
    }

    public static void SubWithCarry(Register register, Register valueToAdd, bool destinationIsIndirect = false, int? destinationDisplacement = null)
    {
      Do<SubWithCarry>(register, valueToAdd, destinationDisplacement: destinationDisplacement, destinationIsIndirect: destinationIsIndirect);
    }

    public static void And(Register register, uint value, bool destinationIsIndirect = false, int? destinationDisplacement = null, RegisterSize size = RegisterSize.Int32)
    {
      Do<And>(register, value, destinationIsIndirect: destinationIsIndirect, destinationDisplacement: destinationDisplacement, size: size);
    }

    public static void And(Register register, Register value, bool destinationIsIndirect = false, int? destinationDisplacement = null, RegisterSize? size = null)
    {
      Do<And>(register, value, destinationIsIndirect: destinationIsIndirect, destinationDisplacement: destinationDisplacement, size: size);
    }

    public static void Xor(string destination, Register source, bool destinationIsIndirect = false, int? destinationDisplacement = null, bool sourceIsIndirect = false, int? sourceDisplacement = null, RegisterSize? size = null)
    {
      Do<Xor>(destination, source, destinationIsIndirect, destinationDisplacement, sourceIsIndirect, sourceDisplacement, size);
    }

    public static void Xor(string destination, UInt32 value, bool destinationIsIndirect = false, int? destinationDisplacement = null, bool sourceIsIndirect = false, int? sourceDisplacement = null, RegisterSize size = RegisterSize.Int32)
    {
      Do<Xor>(destination, value, destinationIsIndirect, destinationDisplacement, sourceIsIndirect, sourceDisplacement, size);
    }

    public static void Xor(string destination, string source, bool destinationIsIndirect = false, int? destinationDisplacement = null, bool sourceIsIndirect = false, int? sourceDisplacement = null, RegisterSize size = RegisterSize.Int32)
    {
      Do<Xor>(destination, source, destinationIsIndirect, destinationDisplacement, sourceIsIndirect, sourceDisplacement, size);
    }

    public static void Xor(Register destination, string sourceLabel, bool destinationIsIndirect = false, int? destinationDisplacement = null, bool sourceIsIndirect = false, int? sourceDisplacement = null, RegisterSize? size = null)
    {
      Do<Xor>(destination, sourceLabel, destinationIsIndirect, destinationDisplacement, sourceIsIndirect, sourceDisplacement, size);
    }

    public static void Xor(Register destination, uint value, bool destinationIsIndirect = false, int? destinationDisplacement = null, bool sourceIsIndirect = false, int? sourceDisplacement = null, RegisterSize? size = null)
    {
      Do<Xor>(destination, value, destinationIsIndirect, destinationDisplacement, sourceIsIndirect, sourceDisplacement, size);
    }

    public static void Xor(Register destination, Register source, bool destinationIsIndirect = false, int? destinationDisplacement = null, bool sourceIsIndirect = false, int? sourceDisplacement = null)
    {
      Do<Xor>(destination, source, destinationIsIndirect, destinationDisplacement, sourceIsIndirect, sourceDisplacement);
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

    public static void Test(Register destination, Register sourceReg, bool sourceIsIndirect = false)
    {
      if (!sourceIsIndirect)
      {
        if (destination.Size != sourceReg.Size)
        {
          throw new InvalidOperationException("Register sizes don't match!");
        }
      }
      new Test
      {
        DestinationReg = destination.RegEnum,
        SourceReg = sourceReg,
        SourceIsIndirect = sourceIsIndirect
      };
    }

    public static void Divide(uint destinationValue, bool isIndirect = false, int? displacement = null, RegisterSize size = RegisterSize.Int32)
    {
      Do<Divide>(destinationValue, isIndirect, displacement, size);
    }

    public static void Divide(Register register, bool isIndirect = false, int? displacement = null, RegisterSize? size = null)
    {
      Do<Divide>(register, isIndirect, displacement, size);
    }

    public static void Divide(string label, bool isIndirect = false, int? displacement = null, RegisterSize size = RegisterSize.Int32)
    {
      Do<Divide>(label, isIndirect, displacement, size);
    }

    public static void IntegerDivide(uint destinationValue, bool isIndirect = false, int? displacement = null, RegisterSize size = RegisterSize.Int32)
    {
      Do<IDivide>(destinationValue, isIndirect, displacement, size);
    }

    public static void IntegerDivide(Register register, bool isIndirect = false, int? displacement = null, RegisterSize? size = null)
    {
      Do<IDivide>(register, isIndirect, displacement, size);
    }

    public static void IntegerDivide(string label, bool isIndirect = false, int? displacement = null, RegisterSize size = RegisterSize.Int32)
    {
      Do<IDivide>(label, isIndirect, displacement, size);
    }

    public static void Multiply(uint destinationValue, bool isIndirect = false, int? displacement = null, RegisterSize size = RegisterSize.Int32)
    {
      Do<Multiply>(destinationValue, isIndirect, displacement, size);
    }

    public static void Multiply(Register register, bool isIndirect = false, int? displacement = null, RegisterSize size = RegisterSize.Int32)
    {
      Do<Multiply>(register, isIndirect, displacement, size);
    }

    public static void Multiply(string label, bool isIndirect = false, int? displacement = null, RegisterSize size = RegisterSize.Int32)
    {
      Do<Multiply>(label, isIndirect, displacement, size);
    }

    public static void Negate(uint destinationValue, bool isIndirect = false, int? displacement = null, RegisterSize size = RegisterSize.Int32)
    {
      Do<Neg>(destinationValue, isIndirect, displacement, size);
    }

    public static void Negate(Register register, bool isIndirect = false, int? displacement = null, RegisterSize? size = null)
    {
      Do<Neg>(register, isIndirect, displacement, size);
    }

    public static void Negate(string label, bool isIndirect = false, int? displacement = null, RegisterSize size = RegisterSize.Int32)
    {
      Do<Neg>(label, isIndirect, displacement, size);
    }

    public static void Not(uint destinationValue, bool isIndirect = false, int? displacement = null, RegisterSize size = RegisterSize.Int32)
    {
      Do<Not>(destinationValue, isIndirect, displacement, size);
    }

    public static void Not(Register register, bool isIndirect = false, int? displacement = null, RegisterSize? size = null)
    {
      Do<Not>(register, isIndirect, displacement, size);
    }

    public static void Not(string label, bool isIndirect = false, int? displacement = null, RegisterSize size = RegisterSize.Int32)
    {
      Do<Not>(label, isIndirect, displacement, size);
    }

    public static void AddWithCarry(string destination, Register source, bool destinationIsIndirect = false, int? destinationDisplacement = null, bool sourceIsIndirect = false, int? sourceDisplacement = null, RegisterSize? size = null)
    {
      Do<AddWithCarry>(destination, source, destinationIsIndirect, destinationDisplacement, sourceIsIndirect, sourceDisplacement, size);
    }

    public static void AddWithCarry(string destination, UInt32 value, bool destinationIsIndirect = false, int? destinationDisplacement = null, bool sourceIsIndirect = false, int? sourceDisplacement = null, RegisterSize size = RegisterSize.Int32)
    {
      Do<AddWithCarry>(destination, value, destinationIsIndirect, destinationDisplacement, sourceIsIndirect, sourceDisplacement, size);
    }

    public static void AddWithCarry(string destination, string source, bool destinationIsIndirect = false, int? destinationDisplacement = null, bool sourceIsIndirect = false, int? sourceDisplacement = null, RegisterSize size = RegisterSize.Int32)
    {
      Do<AddWithCarry>(destination, source, destinationIsIndirect, destinationDisplacement, sourceIsIndirect, sourceDisplacement, size);
    }

    public static void AddWithCarry(Register destination, string sourceLabel, bool destinationIsIndirect = false, int? destinationDisplacement = null, bool sourceIsIndirect = false, int? sourceDisplacement = null, RegisterSize? size = null)
    {
      Do<AddWithCarry>(destination, sourceLabel, destinationIsIndirect, destinationDisplacement, sourceIsIndirect, sourceDisplacement, size);
    }

    public static void AddWithCarry(Register destination, uint value, bool destinationIsIndirect = false, int? destinationDisplacement = null, bool sourceIsIndirect = false, int? sourceDisplacement = null, RegisterSize? size = null)
    {
      Do<AddWithCarry>(destination, value, destinationIsIndirect, destinationDisplacement, sourceIsIndirect, sourceDisplacement, size);
    }

    public static void AddWithCarry(Register destination, Register source, bool destinationIsIndirect = false, int? destinationDisplacement = null, bool sourceIsIndirect = false, int? sourceDisplacement = null)
    {
      Do<AddWithCarry>(destination, source, destinationIsIndirect, destinationDisplacement, sourceIsIndirect, sourceDisplacement);
    }

    public static void Or(string destination, Register source, bool destinationIsIndirect = false, int? destinationDisplacement = null, bool sourceIsIndirect = false, int? sourceDisplacement = null, RegisterSize? size = null)
    {
      Do<Or>(destination, source, destinationIsIndirect, destinationDisplacement, sourceIsIndirect, sourceDisplacement, size);
    }

    public static void Or(string destination, UInt32 value, bool destinationIsIndirect = false, int? destinationDisplacement = null, bool sourceIsIndirect = false, int? sourceDisplacement = null, RegisterSize size = RegisterSize.Int32)
    {
      Do<Or>(destination, value, destinationIsIndirect, destinationDisplacement, sourceIsIndirect, sourceDisplacement, size);
    }

    public static void Or(string destination, string source, bool destinationIsIndirect = false, int? destinationDisplacement = null, bool sourceIsIndirect = false, int? sourceDisplacement = null, RegisterSize size = RegisterSize.Int32)
    {
      Do<Or>(destination, source, destinationIsIndirect, destinationDisplacement, sourceIsIndirect, sourceDisplacement, size);
    }

    public static void Or(Register destination, string sourceLabel, bool destinationIsIndirect = false, int? destinationDisplacement = null, bool sourceIsIndirect = false, int? sourceDisplacement = null, RegisterSize? size = null)
    {
      Do<Or>(destination, sourceLabel, destinationIsIndirect, destinationDisplacement, sourceIsIndirect, sourceDisplacement, size);
    }

    public static void Or(Register destination, uint value, bool destinationIsIndirect = false, int? destinationDisplacement = null, bool sourceIsIndirect = false, int? sourceDisplacement = null, RegisterSize? size = null)
    {
      Do<Or>(destination, value, destinationIsIndirect, destinationDisplacement, sourceIsIndirect, sourceDisplacement, size);
    }

    public static void Or(Register destination, Register source, bool destinationIsIndirect = false, int? destinationDisplacement = null, bool sourceIsIndirect = false, int? sourceDisplacement = null)
    {
      Do<Or>(destination, source, destinationIsIndirect, destinationDisplacement, sourceIsIndirect, sourceDisplacement);
    }

    public static void SignExtendAX(RegisterSize size)
    {
      new SignExtendAX
      {
        Size = (byte)size
      };
    }

    public static void Exchange(Register destination, Register source, bool destinationIsIndirect = false, int? destinationDisplacement = null)
    {
      Do<Xchg>(destination, source, destinationIsIndirect: destinationIsIndirect, destinationDisplacement: destinationDisplacement);
    }

    public static void ClearInterruptFlag()
    {
      new ClearInterruptFlag();
    }

    public static void ClearDirectionFlag()
    {
      new ClrDirFlag();
    }

    public static void DebugNoop()
    {
      new DebugNoop();
    }

    public static void Halt()
    {
      new Halt();
    }

    public static void Int3()
    {
      new INT3();
    }

    public static void Noop()
    {
      new Noop();
    }

    public static void PopAllRegisters()
    {
      new Popad();
    }

    public static void PushAllRegisters()
    {
      new Pushad();
    }

    public static void EnableInterrupts()
    {
      new Sti();
    }

    public static void DisableInterrupts()
    {
      new ClearInterruptFlag();
    }

    public static void StoreByteInString()
    {
      new StoreByteInString();
    }

    public static void StoreWordInString()
    {
      new StoreWordInString();
    }

    public static void LoadGdt(Register32 destination, bool isIndirect = false)
    {
      new Lgdt
      {
        DestinationReg = destination,
        DestinationIsIndirect = isIndirect
      };
    }

    public static void LoadIdt(Register32 destination, bool isIndirect = false)
    {
      new Lidt
      {
        DestinationReg = destination,
        DestinationIsIndirect = isIndirect
      };
    }

    public static void RotateThroughCarryRight(string destination, Register source, bool destinationIsIndirect = false, int? destinationDisplacement = null, bool sourceIsIndirect = false, int? sourceDisplacement = null, RegisterSize? size = null)
    {
      Do<RotateThroughCarryRight>(destination, source, destinationIsIndirect, destinationDisplacement, sourceIsIndirect, sourceDisplacement, size);
    }

    public static void RotateThroughCarryRight(string destination, UInt32 value, bool destinationIsIndirect = false, int? destinationDisplacement = null, bool sourceIsIndirect = false, int? sourceDisplacement = null, RegisterSize size = RegisterSize.Int32)
    {
      Do<RotateThroughCarryRight>(destination, value, destinationIsIndirect, destinationDisplacement, sourceIsIndirect, sourceDisplacement, size);
    }

    public static void RotateThroughCarryRight(string destination, string source, bool destinationIsIndirect = false, int? destinationDisplacement = null, bool sourceIsIndirect = false, int? sourceDisplacement = null, RegisterSize size = RegisterSize.Int32)
    {
      Do<RotateThroughCarryRight>(destination, source, destinationIsIndirect, destinationDisplacement, sourceIsIndirect, sourceDisplacement, size);
    }

    public static void RotateThroughCarryRight(Register destination, string sourceLabel, bool destinationIsIndirect = false, int? destinationDisplacement = null, bool sourceIsIndirect = false, int? sourceDisplacement = null, RegisterSize? size = null)
    {
      Do<RotateThroughCarryRight>(destination, sourceLabel, destinationIsIndirect, destinationDisplacement, sourceIsIndirect, sourceDisplacement, size);
    }

    public static void RotateThroughCarryRight(Register destination, uint value, bool destinationIsIndirect = false, int? destinationDisplacement = null, bool sourceIsIndirect = false, int? sourceDisplacement = null, RegisterSize? size = null)
    {
      Do<RotateThroughCarryRight>(destination, value, destinationIsIndirect, destinationDisplacement, sourceIsIndirect, sourceDisplacement, size);
    }

    public static void RotateThroughCarryRight(Register destination, Register source, bool destinationIsIndirect = false, int? destinationDisplacement = null, bool sourceIsIndirect = false, int? sourceDisplacement = null, RegisterSize? size = null)
    {
      Do<RotateThroughCarryRight>(destination, source, destinationIsIndirect, destinationDisplacement, sourceIsIndirect, sourceDisplacement, size: size);
    }

    public static void ShiftRightDouble(Register destination, Register source, uint argumentValue)
    {
      new ShiftRightDouble()
      {
        DestinationReg = destination,
        SourceReg = source,
        ArgumentValue = argumentValue
      };
    }

    public static void ShiftRightDouble(Register destination, Register source, Register8 argumentReg, bool destinationIsIndirect = false, int? destinationDisplacement = null)
    {
      if (argumentReg != CL)
      {
        throw new InvalidOperationException("Argument needs to be CL!");
      }
      if (destinationDisplacement != null)
      {
        destinationIsIndirect = true;
        if (destinationDisplacement == 0)
        {
          destinationDisplacement = null;
        }
      }
      new ShiftRightDouble()
      {
        DestinationReg = destination,
        DestinationIsIndirect = destinationIsIndirect,
        DestinationDisplacement = destinationDisplacement,
        SourceReg = source,
        ArgumentReg = argumentReg
      };
    }

    public static void ShiftLeftDouble(Register destination, Register source, uint argumentValue)
    {
      new ShiftRightDouble()
      {
        DestinationReg = destination,
        SourceReg = source,
        ArgumentValue = argumentValue
      };
    }

    public static void ShiftLeftDouble(Register destination, Register source, Register8 argumentReg, bool destinationIsIndirect = false, int? destinationDisplacement = null)
    {
      if (argumentReg != CL)
      {
        throw new InvalidOperationException("Argument needs to be CL!");
      }
      if (destinationDisplacement != null)
      {
        destinationIsIndirect = true;
        if (destinationDisplacement == 0)
        {
          destinationDisplacement = null;
        }
      }
      new ShiftLeftDouble()
      {
        DestinationReg = destination,
        DestinationIsIndirect = destinationIsIndirect,
        DestinationDisplacement = destinationDisplacement,
        SourceReg = source,
        ArgumentReg = argumentReg
      };
    }

    public static void JumpToSegment(ushort segment, string targetLabel)
    {
      new JumpToSegment
      {
        Segment = segment,
        DestinationLabel = targetLabel
      };
    }

    #region MoveSignExtend

    public static void MoveSignExtend(string destination, Register source, bool destinationIsIndirect = false, int? destinationDisplacement = null, bool sourceIsIndirect = false, int? sourceDisplacement = null, RegisterSize? size = null)
    {
      Do<MoveSignExtend>(destination, source, destinationIsIndirect, destinationDisplacement, sourceIsIndirect, sourceDisplacement, size);
    }

    public static void MoveSignExtend(string destination, UInt32 value, bool destinationIsIndirect = false, int? destinationDisplacement = null, bool sourceIsIndirect = false, int? sourceDisplacement = null, RegisterSize size = RegisterSize.Int32)
    {
      Do<MoveSignExtend>(destination, value, destinationIsIndirect, destinationDisplacement, sourceIsIndirect, sourceDisplacement, size);
    }

    public static void MoveSignExtend(string destination, string source, bool destinationIsIndirect = false, int? destinationDisplacement = null, bool sourceIsIndirect = false, int? sourceDisplacement = null, RegisterSize size = RegisterSize.Int32)
    {
      Do<MoveSignExtend>(destination, source, destinationIsIndirect, destinationDisplacement, sourceIsIndirect, sourceDisplacement, size);
    }

    public static void MoveSignExtend(Register destination, string sourceLabel, bool destinationIsIndirect = false, int? destinationDisplacement = null, bool sourceIsIndirect = false, int? sourceDisplacement = null, RegisterSize? size = null)
    {
      Do<MoveSignExtend>(destination, sourceLabel, destinationIsIndirect, destinationDisplacement, sourceIsIndirect, sourceDisplacement, size);
    }

    public static void MoveSignExtend(Register destination, uint value, bool destinationIsIndirect = false, int? destinationDisplacement = null, bool sourceIsIndirect = false, int? sourceDisplacement = null, RegisterSize? size = null)
    {
      Do<MoveSignExtend>(destination, value, destinationIsIndirect, destinationDisplacement, sourceIsIndirect, sourceDisplacement, size);
    }

    public static void MoveSignExtend(Register destination, Register source, bool destinationIsIndirect = false, int? destinationDisplacement = null, bool sourceIsIndirect = false, int? sourceDisplacement = null, RegisterSize? size = null)
    {
      Do<MoveSignExtend>(destination, source, destinationIsIndirect, destinationDisplacement, sourceIsIndirect, sourceDisplacement, skipSizeCheck: true);
    }

    #endregion MoveSignExtend

    #region MoveZeroExtend

    public static void MoveZeroExtend(string destination, Register source, bool destinationIsIndirect = false, int? destinationDisplacement = null, bool sourceIsIndirect = false, int? sourceDisplacement = null, RegisterSize? size = null)
    {
      Do<MoveZeroExtend>(destination, source, destinationIsIndirect, destinationDisplacement, sourceIsIndirect, sourceDisplacement, size);
    }

    public static void MoveZeroExtend(string destination, UInt32 value, bool destinationIsIndirect = false, int? destinationDisplacement = null, bool sourceIsIndirect = false, int? sourceDisplacement = null, RegisterSize size = RegisterSize.Int32)
    {
      Do<MoveZeroExtend>(destination, value, destinationIsIndirect, destinationDisplacement, sourceIsIndirect, sourceDisplacement, size);
    }

    public static void MoveZeroExtend(string destination, string source, bool destinationIsIndirect = false, int? destinationDisplacement = null, bool sourceIsIndirect = false, int? sourceDisplacement = null, RegisterSize size = RegisterSize.Int32)
    {
      Do<MoveZeroExtend>(destination, source, destinationIsIndirect, destinationDisplacement, sourceIsIndirect, sourceDisplacement, size);
    }

    public static void MoveZeroExtend(Register destination, string sourceLabel, bool destinationIsIndirect = false, int? destinationDisplacement = null, bool sourceIsIndirect = false, int? sourceDisplacement = null, RegisterSize? size = null)
    {
      Do<MoveZeroExtend>(destination, sourceLabel, destinationIsIndirect, destinationDisplacement, sourceIsIndirect, sourceDisplacement, size);
    }

    public static void MoveZeroExtend(Register destination, uint value, bool destinationIsIndirect = false, int? destinationDisplacement = null, bool sourceIsIndirect = false, int? sourceDisplacement = null, RegisterSize? size = null)
    {
      Do<MoveZeroExtend>(destination, value, destinationIsIndirect, destinationDisplacement, sourceIsIndirect, sourceDisplacement, size);
    }

    public static void MoveZeroExtend(Register destination, Register source, bool destinationIsIndirect = false, int? destinationDisplacement = null, bool sourceIsIndirect = false, int? sourceDisplacement = null, RegisterSize? size = null)
    {
      Do<MoveZeroExtend>(destination, source, destinationIsIndirect, destinationDisplacement, sourceIsIndirect, sourceDisplacement, skipSizeCheck: true);
    }

    #endregion MoveZeroExtend

    public static void Cpuid()
    {
      new CpuId();
    }

    public static void Rdtsc()
    {
      new Rdtsc();
    }

    public static void Rdmsr()
    {
      new Rdmsr();
    }
  }
}
