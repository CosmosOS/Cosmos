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
    , ValueInt
    // Symbols
    // _ and . are AlphaNum. See comments in Parser
    , Equals, BracketLeft, BracketRight, Plus, Minus, Colon, Dollar, CurlyLeft, CurlyRight, Comma, LessThan, GreaterThan, Question, At
    // 
    , WhiteSpace
    // For now used during scanning while user is typing, but in future could be user methods we have to find etc
    , Unknown
    }

  public class Token {
    public TokenType Type = TokenType.Unknown;
    public string Value;
    public int SrcPosStart;
    public int SrcPosEnd;
  }
}
