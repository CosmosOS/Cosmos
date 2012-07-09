using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.Compiler.XSharp {
  public enum TokenType { 
    // Line based
    Comment, LiteralAsm
    //
    , Register, Keyword, AlphaNum
    // Values
    , ValueInt, ValueString
    // 
    , WhiteSpace, Operator, Delimiter
    // For now used during scanning while user is typing, but in future could be user methods we have to find etc
    , Unknown
    }

  public class Token {
    public TokenType Type = TokenType.Unknown;
    public string Value;
    public int SrcPosStart;
    public int SrcPosEnd;

    public override string ToString() {
      return Value;
    }
  }
}
