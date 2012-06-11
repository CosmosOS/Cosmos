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
      } else {
        xString = mData.Substring(mStart, rPos - mStart);

        if (xString == "++") {
          xToken.Type = Token.TokenType.Inc;
        } else if (xString == "--") {
          xToken.Type = Token.TokenType.Dec;
        } else if (xString == "[") {
          xToken.Type = Token.TokenType.BracketLeft;
        } else if (xString == "]") {
          xToken.Type = Token.TokenType.BracketRight;
        } else if (xString == "+") {
          xToken.Type = Token.TokenType.Plus;
        } else if (xString == "-") {
          xToken.Type = Token.TokenType.Minus;
        } else if (xString == "=") {
          xToken.Type = Token.TokenType.Assignment;
        } else if (char.IsLetter(xChar1)) {
          xToken.Type = Token.TokenType.Text;
        } else if (char.IsDigit(xChar1)) {
          xToken.Type = Token.TokenType.ValueNumber;
        } else {
          throw new Exception("Unrecognized character.");
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
      bool xLastIsWhiteSpace = true;
      bool xLastIsLetterOrDigit = false;
      bool xLastIsOther = false;
      for (i = 0; i < mData.Length; i++) {
        char xChar = mData[i];
        bool xIsWhiteSpace = char.IsWhiteSpace(xChar);
        bool xIsLetterOrDigit = char.IsLetterOrDigit(xChar);
        bool xIsOther = !xIsWhiteSpace && !xIsLetterOrDigit;

        if (xLastIsWhiteSpace) {
          mStart = i;
        } else {
          if (xIsWhiteSpace) {
            NewToken(ref i);
          } else if (xIsLetterOrDigit && !xLastIsLetterOrDigit) {
            NewToken(ref i);
          } else if (xIsOther && !xLastIsOther) { 
            NewToken(ref i);
          }
        }

        xLast = xChar;
        xLastIsWhiteSpace = xIsWhiteSpace;
        xLastIsLetterOrDigit = xIsLetterOrDigit;
        xLastIsOther = xIsOther;
      }
    }

    public Parser(string aData) {
      // We add a dummy whitespace to force the parser to trigger a NewToken for the last item.
      mData = aData + ' ';
      Parse();
    }
  }
}
