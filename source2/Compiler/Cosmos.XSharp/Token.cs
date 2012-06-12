using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.Compiler.XSharp {
  public enum TokenType { 
    // Line based
    Comment, LiteralAsm, 
    //
    Register, Label, OpCode, ValueNumber, 
    // Symbols
    Assignment, BracketLeft, BracketRight, Plus, Minus, Inc, Dec, 
    // 
    WhiteSpace,
    // For now used during scanning while user is typing, but in future could be user methods we have to find etc
    Unknown
    }

  public class Token {
    public TokenType Type;
    public int SrcPosStart;
    public int SrcPosEnd;
    public string Value;
  }
}
