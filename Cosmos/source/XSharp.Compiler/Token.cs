using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace XSharp.Compiler {
  public enum TokenType { 
    // Line based
    Comment, LiteralAsm
    //
    , Register, Keyword, AlphaNum
    // Values
    , ValueInt, ValueString
    // 
    , WhiteSpace, Operator, Delimiter
    , Call
    // For now used during scanning while user is typing, but in future could be user methods we have to find etc
    , Unknown
    }

  public class Token {
    public TokenType Type = TokenType.Unknown;
    public string Value;
    public int SrcPosStart;
    public int SrcPosEnd;

    /// <summary>Get line number this token belongs to.</summary>
    public int LineNumber { get; private set; }

    public Token(int lineNumber) {
        LineNumber = lineNumber;
        return;
    }

    public override string ToString() {
      return Value;
    }

    static public implicit operator string(Token aToken) {
      return aToken.Value;
    }

    public bool Matches(string aText) {
      return string.Equals(Value, aText, StringComparison.InvariantCultureIgnoreCase);
    }
  }
}
