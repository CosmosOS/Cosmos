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

    public static void Set32(string destination, string sourceLabel, bool sourceIsIndirect = false)
    {
      new Mov
      {
        Size = 32,
        DestinationRef = ElementReference.New(destination),
        DestinationIsIndirect = true,
        SourceRef = ElementReference.New(sourceLabel),
        SourceIsIndirect = sourceIsIndirect
      };
    }

    public static void Set(Register destination, string sourceLabel, bool destinationIsIndirect = false, bool sourceIsIndirect = false)
    {
      new Mov
      {
        Size = (byte)destination.Size,
        DestinationReg = destination.RegEnum,
        DestinationIsIndirect = destinationIsIndirect,
        SourceRef = ElementReference.New(sourceLabel),
        SourceIsIndirect = sourceIsIndirect
      };
    }

    public static void Set(string destination, Register source, bool destinationIsIndirect = false, bool sourceIsIndirect = false)
    {
      new Mov
      {
        Size = (byte)source.Size,
        DestinationRef = ElementReference.New(destination),
        DestinationIsIndirect = destinationIsIndirect,
        SourceReg = source.RegEnum,
        SourceIsIndirect = sourceIsIndirect,
      };
    }

    public static void Set(RegisterSize size, string destination, string source, bool destinationIsIndirect = false, bool sourceIsIndirect = false)
    {
      new Mov
      {
        Size = (byte)size,
        DestinationRef = ElementReference.New(destination),
        DestinationIsIndirect = destinationIsIndirect,
        SourceRef = ElementReference.New(source),
        SourceIsIndirect = sourceIsIndirect
      };
    }

    public static void Set(string destination, UInt32 value, bool destinationIsIndirect = false, RegisterSize size = RegisterSize.Int32)
    {
      new Mov
      {
        Size = (byte)size,
        DestinationRef = ElementReference.New(destination),
        DestinationIsIndirect = destinationIsIndirect,
        SourceValue = value
      };
    }

    public static void Set(Register destination, int destinationDisplacement, string sourceLabel, bool sourceIsIndirect = false)
    {
      new Mov
      {
        Size = (byte)destination.Size,
        DestinationReg = destination.RegEnum,
        DestinationIsIndirect = true,
        SourceRef = ElementReference.New(sourceLabel),
        SourceIsIndirect = sourceIsIndirect
      };
    }

    public static void Set(Register destination, uint value, bool sourceIsIndirect = false)
    {
      new Mov
      {
        Size = (byte)destination.Size,
        DestinationReg = destination.RegEnum,
        SourceValue = value,
      };
    }

    public static void Set(Register destination,
                           Register source,
                           bool destinationIsIndirect = false,
                           int? destinationDisplacement = null,
                           bool sourceIsIndirect = false,
                           int? sourceDisplacement = null)
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

      new Mov
      {
        Size = (byte)xSize,
        DestinationReg = destination.RegEnum,
        DestinationIsIndirect = destinationIsIndirect,
        DestinationDisplacement = destinationDisplacement.GetValueOrDefault(),
        SourceIsIndirect = sourceIsIndirect,
        SourceDisplacement = sourceDisplacement.GetValueOrDefault(),
        SourceReg = source.RegEnum
      };
    }

    public static void SetByte(uint address, byte value)
    {
      new Mov { DestinationValue = address, DestinationIsIndirect = true, SourceValue = value };
    }

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
      Assembler.CurrentInstance.DataMembers.Add(new DataMember(name, "`" + value + "`"));
    }

    public static void DataMember(string name, uint elementCount, string size, string value)
    {
      new LiteralAssemblerCode(name + ": TIMES " + elementCount + " " + size + " " + value);
    }

    public static void RotateRight(Register register, uint bitCount)
    {
      new RotateRight { DestinationReg = register.RegEnum, SourceValue = bitCount, Size = (byte)register.Size };
    }

    public static void RotateLeft(Register register, uint bitCount)
    {
      new RotateLeft { DestinationReg = register.RegEnum, SourceValue = bitCount, Size = (byte)register.Size };
    }

    public static void ShiftRight(Register register, uint bitCount)
    {
      new ShiftRight { DestinationReg = register.RegEnum, SourceValue = bitCount, Size = (byte)register.Size };
    }

    public static void ShiftLeft(Register register, uint bitCount)
    {
      new ShiftLeft { DestinationReg = register.RegEnum, SourceValue = bitCount, Size = (byte)register.Size };
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
      new Push
      {
        DestinationReg = value.RegEnum
      };
    }

    public static void Push(uint value, RegisterSize size)
    {
      new Push
      {
        DestinationValue = value,
        Size = (byte)size
      };
    }

    public static void Push(string label, bool isIndirect = false, RegisterSize size = RegisterSize.Int32)
    {
      new Push
      {
        DestinationRef = ElementReference.New(label),
        DestinationIsIndirect = isIndirect,
        Size = (byte)size
      };
    }

    public static void Pop(Register value)
    {
      new Pop
      {
        DestinationReg = value.RegEnum
      };
    }

    public static void Increment(Register value)
    {
      new INC()
      {
        DestinationReg = value.RegEnum
      };
    }

    public static void Decrement(Register value)
    {
      new Dec()
      {
        DestinationReg = value.RegEnum
      };
    }

    public static void Add(Register register, uint valueToAdd)
    {
      new Add
      {
        DestinationReg = register.RegEnum,
        SourceValue = valueToAdd
      };
    }

    public static void Add(Register register, Register valueToAdd)
    {
      new Add
      {
        DestinationReg = register.RegEnum,
        SourceReg = valueToAdd.RegEnum
      };
    }

    public static void Sub(Register register, uint valueToAdd)
    {
      new Sub
      {
        DestinationReg = register.RegEnum,
        SourceValue = valueToAdd
      };
    }

    public static void Sub(Register register, Register valueToAdd)
    {
      new Sub
      {
        DestinationReg = register.RegEnum,
        SourceReg = valueToAdd.RegEnum
      };
    }

    public static void And(Register register, uint valueToAdd)
    {
      new And
      {
        DestinationReg = register.RegEnum,
        SourceValue = valueToAdd
      };
    }

    public static void And(Register register, Register valueToAdd)
    {
      new And
      {
        DestinationReg = register.RegEnum,
        SourceReg = valueToAdd.RegEnum
      };
    }

    public static void Or(Register register, uint valueToAdd)
    {
      new Or
      {
        DestinationReg = register.RegEnum,
        SourceValue = valueToAdd
      };
    }

    public static void Or(Register register, Register valueToAdd)
    {
      new Or
      {
        DestinationReg = register.RegEnum,
        SourceReg = valueToAdd.RegEnum
      };
    }

    public static void Xor(Register register, uint valueToAdd)
    {
      new Xor
      {
        DestinationReg = register.RegEnum,
        SourceValue = valueToAdd
      };
    }

    public static void Xor(Register register, Register valueToAdd)
    {
      new Xor
      {
        DestinationReg = register.RegEnum,
        SourceReg = valueToAdd.RegEnum
      };
    }

    public static void IntegerMultiply(Register register, uint valueToAdd)
    {
      new Imul
      {
        DestinationReg = register.RegEnum,
        SourceValue = valueToAdd
      };
    }

    public static void IntegerMultiply(Register register, Register valueToAdd)
    {
      if (register.Size != valueToAdd.Size)
      {
        throw new Exception("Registers need to be the same size!");
      }
      new Imul
      {
        DestinationReg = register.RegEnum,
        SourceReg = valueToAdd.RegEnum
      };
    }
  }
}
