using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.Compiler.XSharp {
  public class Parser {
    protected int mStart = 0;
    protected string mData;
    protected bool mIncludeWhiteSpace;

    protected List<Token> mTokens = new List<Token>();
    public List<Token> Tokens {
      get { return mTokens; }
    }

    protected static string[] mOps = new string[] { 
      "POP", "POPALL", "PUSH", "PUSHALL"
    };
    protected static string[] mRegisters = new string[] { 
      "EAX", "AX", "AH", "AL",
      "EBX", "BX", "BH", "BL",
      "ECX", "CX", "CH", "CL",
      "EDX", "DX", "DH", "DL",
      "ESI", "EDI", "ESP", "EBP"
    };

    protected void NewToken(ref int rPos) {
      string xString = null;
      char xChar1 = mData[mStart];
      var xToken = new Token();
      if ((mTokens.Count == 0) && "#!".Contains(xChar1)) {
        rPos = mData.Length; // This will account for the dummy whitespace at the end.
        xString = mData.Substring(mStart + 1, rPos - mStart - 1).Trim();
        if (xChar1 == '#') {
          xToken.Type = TokenType.Comment;
        } else if (xChar1 == '!') {
          xToken.Type = TokenType.LiteralAsm;
        }
      } else {
        xString = mData.Substring(mStart, rPos - mStart);

        if (string.IsNullOrWhiteSpace(xString) && xString.Length > 0) {
          if (mIncludeWhiteSpace) {
            xToken.Type = TokenType.WhiteSpace;
          } else {
            mStart = rPos;
            return;
          }
        } else if (char.IsLetter(xChar1)) {
          string xUpper = xString.ToUpper();
          if (mRegisters.Contains(xUpper)) {
            xToken.Type = TokenType.Register;
          } else if (mOps.Contains(xUpper)) {
            xToken.Type = TokenType.OpCode;
          } else {
            xToken.Type = TokenType.AlphaNum;
          }
        } else if (char.IsDigit(xChar1)) {
          xToken.Type = TokenType.ValueInt;
        } else if (xString == "[") {
          xToken.Type = TokenType.BracketLeft;
        } else if (xString == "]") {
          xToken.Type = TokenType.BracketRight;
        } else if (xString == "+") {
          xToken.Type = TokenType.Plus;
        } else if (xString == "-") {
          xToken.Type = TokenType.Minus;
        } else if (xString == "=") {
          xToken.Type = TokenType.Assignment;
        } else if (xString == ":") {
          xToken.Type = TokenType.Colon;
        } else if (xString == "$") {
          xToken.Type = TokenType.Dollar;
        } else {
          xToken.Type = TokenType.Unknown;
        }
      }
      xToken.Value = xString;
      xToken.SrcPosStart = mStart;
      xToken.SrcPosEnd = rPos - 1;
      // Do near end, some logic performs returns above
      mTokens.Add(xToken);
      mStart = rPos;
    }

    protected enum CharType { WhiteSpace, Identifier, Symbol };
    protected void Parse() {
      char xLastChar = ' ';
      CharType xLastCharType = CharType.WhiteSpace;
      char xChar;
      CharType xCharType = CharType.WhiteSpace;
      int i = 0;
      for (i = 0; i < mData.Length; i++) {
        xChar = mData[i];
        if (char.IsWhiteSpace(xChar)) {
          xCharType = CharType.WhiteSpace;
        } else if (char.IsLetterOrDigit(xChar)) {
          xCharType = CharType.Identifier;
        } else {
          xCharType = CharType.Symbol;
        }

        // i > 0 - Never do NewToken on first char. i = 0 is just a pass to get char and set lastchar.
        // But its faster as the second short circuit rather than a separate if.
        if (xCharType != xLastCharType && i > 0) {
          NewToken(ref i);
        }

        xLastChar = xChar;
        xLastCharType = xCharType;
      }
      // Last token
      if (mStart < mData.Length) {
        NewToken(ref i);
      }
    }

    public Parser(string aData, bool aIncludeWhiteSpace) {
      mData = aData;
      mIncludeWhiteSpace = aIncludeWhiteSpace;
      Parse();
    }
  }
}
