using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.Compiler.XSharp {
  public class Token {
    public enum TokenType { Comment, Literal, Text, Assignment, ValueNumber, BracketLeft, BracketRight, Plus, Minus, Inc, Dec}

    public TokenType Type;
    public int SrcPosStart;
    public int SrcPosEnd;
    public string Value;
  }
}
