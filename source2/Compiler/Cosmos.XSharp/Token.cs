using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.Compiler.XSharp {
  public enum TokenType { 
    // Line based
    Comment, LiteralAsm
    //
    , Register, OpCode, AlphaNum
    // Values
    , ValueInt
    // Symbols
    , Assignment, BracketLeft, BracketRight, Plus, Minus, Colon, Dollar
    // 
    , WhiteSpace
    // For now used during scanning while user is typing, but in future could be user methods we have to find etc
    , Unknown
    }

  public class Token {
    public TokenType Type;
    public int SrcPosStart;
    public int SrcPosEnd;
    public string Value;

    public static TokenType GetTypeForSymbol(string aSymbol) {
      if (aSymbol == "[") {
        return TokenType.BracketLeft;
      } else if (aSymbol == "]") {
        return TokenType.BracketRight;
      } else if (aSymbol == "+") {
        return TokenType.Plus;
      } else if (aSymbol == "-") {
        return TokenType.Minus;
      } else if (aSymbol == "=") {
        return TokenType.Assignment;
      } else if (aSymbol == ":") {
        return TokenType.Colon;
      } else if (aSymbol == "$") {
        return TokenType.Dollar;
      } else {
        return TokenType.Unknown;
      }
    }
  }
}
