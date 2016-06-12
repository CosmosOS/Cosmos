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

    //public static void CompareLiteral(string size, string destination, string source)
    //{
    //  if (string.IsNullOrWhiteSpace(size))
    //  {
    //    LiteralCode($"Cmp {destination}, {source}");
    //  }
    //  else
    //  {
    //    LiteralCode($"Cmp {size} {destination}, {source}");
    //  }
    //}

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
