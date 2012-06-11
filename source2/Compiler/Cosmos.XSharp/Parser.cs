using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.Compiler.XSharp {
  public class Parser {
    protected string mData;
    protected List<Token> mTokens = new List<Token>();
    public List<Token> Tokens {
      get { return mTokens; }
    }

    int mStart = 0;

    protected void NewToken(ref int rPos) {
      string xString = null;
      char xChar1 = mData[mStart];
      var xToken = new Token();
      if ((mTokens.Count == 0) && "#!".Contains(xChar1)) {
        rPos = mData.Length; // This will account for the dummy whitespace at the end.
        xString = mData.Substring(mStart, rPos - mStart);
        if (xChar1 == '#') {
          xToken.Type = Token.TokenType.Comment;
        } else if (xChar1 == '!') {
          xToken.Type = Token.TokenType.Literal;
        }
      } else if (xToken == null) {
        xString = mData.Substring(mStart, rPos - mStart);

        if (xString == "++") {
          xToken.Type = Token.TokenType.Inc;
        } else if (char.IsLetter(xChar1)) {
          xToken.Type = Token.TokenType.Register;
        }
      }
      xToken.Value = xString;
      xToken.SrcPosStart = mStart;
      xToken.SrcPosEnd = rPos - 1;
      mTokens.Add(xToken);
      mStart = rPos;
    }

    protected void Parse() {
      char xLast = ' ';
      int i = 0;
      for (i = 0; i < mData.Length; i++) {
        char xChar = mData[i];
        if (char.IsWhiteSpace(xChar)) {
          if (!char.IsWhiteSpace(xLast)) {
            NewToken(ref i);
          }
        } if (char.IsLetterOrDigit(xChar)) {
          if (!char.IsLetterOrDigit(xLast)) {
            NewToken(ref i);
          }
        } else {
          if (char.IsLetterOrDigit(xLast) || char.IsWhiteSpace(xLast))  {
            NewToken(ref i);
          }
        }
        xLast = xChar;
      }
    }

    public Parser(string aData) {
      // We add a dummy whitespace to force the parser to trigger a NewToken for the last item.
      mData = aData + ' ';
      Parse();
    }
  }
}
