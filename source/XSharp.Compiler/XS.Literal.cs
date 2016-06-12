using System;
using Cosmos.Assembler;
using static XSharp.Compiler.XSRegisters;

namespace XSharp.Compiler
{
  partial class XS
  {
    public static void LiteralCode(string code)
    {
      new LiteralAssemblerCode(code);
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

    public static void PopLiteral(string value)
    {
      LiteralCode("Pop " + value);
    }

    public static void IntegerMultiplyLiteral(string left, string right)
    {
      LiteralCode("imul " + left + ", " + right);
    }

    public static void SetLiteral(string destination, string source)
    {
      LiteralCode("Mov " + destination + ", " + source);
    }

    public static void SetLiteral(string size, string destination, string source)
    {
      LiteralCode("Mov " + size + " " + destination + ", " + source);
    }

    public static void CompareLiteral(string size, string destination, string source)
    {
      if (string.IsNullOrWhiteSpace(size))
      {
        LiteralCode($"Cmp {destination}, {source}");
      }
      else
      {
        LiteralCode($"Cmp {size} {destination}, {source}");
      }
    }

    public static void TestLiteral(string size, string destination, string source)
    {
      if (string.IsNullOrWhiteSpace(size))
      {
        LiteralCode($"Test {destination}, {source}");
      }
      else
      {
        LiteralCode($"Test {size} {destination}, {source}");
      }
    }
  }
}
