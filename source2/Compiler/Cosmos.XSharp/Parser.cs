using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.Compiler.XSharp {
  public class Parser {
    protected int mStart = 0;
    protected string mData;
    protected bool mIncludeWhiteSpace;
    protected bool mAllWhitespace;

    protected TokenList mTokens;
    public TokenList Tokens {
      get { return mTokens; }
    }

    protected static string[] mKeywords = new string[] { 
      "CALL"
      , "END", "EXIT"
      , "GROUP"
      , "INTERRUPTHANDLER"
      , "JUMP"
      , "POPALL", "PUSHALL", "PROCEDURE"
      , "RETURN", "RETURNINTERRUPT"
    };
    protected static string[] mRegisters = new string[] { 
      "EAX", "AX", "AH", "AL",
      "EBX", "BX", "BH", "BL",
      "ECX", "CX", "CH", "CL",
      "EDX", "DX", "DH", "DL",
      "ESI", "EDI", "ESP", "EBP"
    };

    protected Token NewToken(ref int rPos) {
      string xString = null;
      char xChar1 = mData[mStart];
      var xToken = new Token();
      if (mAllWhitespace && "#!".Contains(xChar1)) {
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
          xToken.Type = TokenType.WhiteSpace;
        } else if (char.IsLetter(xChar1)) {
          string xUpper = xString.ToUpper();
          if (mRegisters.Contains(xUpper)) {
            xToken.Type = TokenType.Register;
          } else if (mKeywords.Contains(xUpper)) {
            xToken.Type = TokenType.Keyword;
          } else {
            xToken.Type = TokenType.AlphaNum;
          }
        } else if (char.IsDigit(xChar1)) {
          xToken.Type = TokenType.ValueInt;
        } else if (xString == "[") {
          xToken.Type = TokenType.BracketLeft;
        } else if (xString == "]") {
          xToken.Type = TokenType.BracketRight;
        } else if (xString == "{") {
          xToken.Type = TokenType.CurlyLeft;
        } else if (xString == "}") {
          xToken.Type = TokenType.CurlyRight;
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
        } else if (xString == ".") {
          xToken.Type = TokenType.Dot;
        } else {
          xToken.Type = TokenType.Unknown;
        }
      }
      xToken.Value = xString;
      xToken.SrcPosStart = mStart;
      xToken.SrcPosEnd = rPos - 1;
      if (mAllWhitespace && xToken.Type != TokenType.WhiteSpace) {
        mAllWhitespace = false;
      }
      mStart = rPos;
      return xToken;
    }

    protected enum CharType { WhiteSpace, Identifier, Symbol };
    protected void Parse() {
      var xTokens = ParseText();
      xTokens = ParseTokens(xTokens);
      mTokens = new TokenList(xTokens);
    }

    // Rescan token patterns
    protected List<Token> ParseTokens(List<Token> aTokens) {
      var xResult = new List<Token>();

      for (int i = 0; i < aTokens.Count; i++) {
        int xRemainingTokens = aTokens.Count - i;
        var xToken = aTokens[i];
        if (xToken.Type == TokenType.WhiteSpace && mIncludeWhiteSpace == false) {
        } else {
          // $FF, $02, etc
          if (xToken.Type == TokenType.Dollar && xRemainingTokens > 1) {
            // Dont worry about whitespace, $ FF is not valid, $FF is.
            var xNext = aTokens[i + 1];
            if (xNext.Type == TokenType.ValueInt || xNext.Type == TokenType.AlphaNum) {
              i++;
              xToken.Type = TokenType.ValueInt;
              xToken.SrcPosEnd = xNext.SrcPosEnd;
              xToken.Value = "0x" + xNext.Value;
            }
          }
          xResult.Add(xToken);
        }
      }
      return xResult;
    }

    // Initial Parse to convert text to tokens
    protected List<Token> ParseText() {
      var xResult = new List<Token>();
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
          xResult.Add(NewToken(ref i));
        }

        xLastChar = xChar;
        xLastCharType = xCharType;
      }

      // Last token
      if (mStart < mData.Length) {
        xResult.Add(NewToken(ref i));
      }

      return xResult;
    }

    public Parser(string aData, bool aIncludeWhiteSpace) {
      mData = aData;
      mIncludeWhiteSpace = aIncludeWhiteSpace;
      mAllWhitespace = true;

      Parse();
    }
  }
}
