using System;
using System.Diagnostics.CodeAnalysis;
using Cosmos.Assembler;
using Cosmos.Assembler.x86;
using static XSharp.Compiler.XSRegisters;

namespace XSharp.Compiler
{
  [SuppressMessage("ReSharper", "ObjectCreationAsStatement")]
  public static class XS
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

    public static void Set(string destination, string sourceLabel, bool sourceIsIndirect = false)
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

    public static void Set(Register destination, uint value, bool sourceIsIndirect = false)
    {
      new Mov
      {
        Size = (byte)destination.Size,
        DestinationReg = destination.RegEnum,
        SourceValue = value,
      };
    }

    public static void SetByte(uint address, byte value)
    {
      new Mov { DestinationValue = address, DestinationIsIndirect = true, SourceValue = value };
    }

    public static void SetLiteral(string destination, string source)
    {
      new LiteralAssemblerCode("Mov " + destination + ", " + source);
    }

    public static void SetLiteral(string size, string destination, string source)
    {
      new LiteralAssemblerCode("Mov " + size + " " + destination + ", " + source);
    }

    public static void CompareLiteral(string size, string destination, string source)
    {
      if (string.IsNullOrWhiteSpace(size))
      {
        new LiteralAssemblerCode($"Cmp {destination}, {source}");
      }
      else
      {
        new LiteralAssemblerCode($"Cmp {size} {destination}, {source}");
      }
    }

    public static void TestLiteral(string size, string destination, string source)
    {
      if (string.IsNullOrWhiteSpace(size))
      {
        new LiteralAssemblerCode($"Test {destination}, {source}");
      }
      else
      {
        new LiteralAssemblerCode($"Test {size} {destination}, {source}");
      }
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

    public static void PushLiteral(string value)
    {
      new LiteralAssemblerCode("Push " + value);
    }

    public static void PopLiteral(string value)
    {
      new LiteralAssemblerCode("Pop " + value);
    }

    public static void IntegerMultiplyLiteral(string left, string right)
    {
      new LiteralAssemblerCode("imul " + left + ", " + right);
    }

    public static void LiteralCode(string code)
    {
      new LiteralAssemblerCode(code);
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

    public static void WriteLiteralToPortDX(string value)
    {
      LiteralCode("out DX, " + value);
    }

    public static void ReadLiteralFromPortDX(string value)
    {
      LiteralCode("IN " + value + ", DX");
    }

    public static void PushAllGeneralRegisters()
    {
      new Pushad();
    }

    public static void PopAllGeneralRegisters()
    {
      new Popad();
    }

    public static void AddLiteral(string left, string right)
    {
      LiteralCode("Add " + left + ", " + right);
    }

    public static void SubLiteral(string left, string right)
    {
      LiteralCode("Sub " + left + ", " + right);
    }

    public static void IncrementLiteral(string value)
    {
      LiteralCode("Inc " + value);
    }

    public static void DecrementLiteral(string value)
    {
      LiteralCode("Dec " + value);
    }

    public static void AndLiteral(string left, string right)
    {
      LiteralCode("And " + left + ", " + right);
    }

    public static void OrLiteral(string left, string right)
    {
      LiteralCode("Or " + left + ", " + right);
    }

    public static void XorLiteral(string left, string right)
    {
      LiteralCode("xor " + left + ", " + right);
    }
  }
}
