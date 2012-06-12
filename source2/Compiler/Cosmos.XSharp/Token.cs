using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.Compiler.XSharp {
  public enum TokenType { Comment, Literal, Register, Label, Op, Assignment, ValueNumber, BracketLeft, BracketRight, Plus, Minus, Inc, Dec, WhiteSpace }

  public class Token {
    public TokenType Type;
    public int SrcPosStart;
    public int SrcPosEnd;
    public string Value;
  }
}
