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
    protected bool mAllowPatterns;

    protected TokenList mTokens;
    public TokenList Tokens {
      get { return mTokens; }
    }

    protected static readonly char[] mComma = ",".ToCharArray();
    public static string[] mKeywords = (
      "CALL"
      + ",END,EXIT"
      + ",GOTO,GROUP"
      + ",IF,INTERRUPTHANDLER"
      + ",JUMP"
      + ",POPALL,PUSHALL,PROCEDURE,PORT"
      + ",RETURN,RETURNINTERRUPT"
    ).Split(mComma);

    public static readonly string[] Registers;
    public static readonly string[] Registers8 = "AH,AL,BH,BL,CH,CL,DH,DL".Split(mComma);
    public static readonly string[] Registers16 = "AX,BX,CX,DX".Split(mComma);
    public static readonly string[] Registers32 = "EAX,EBX,ECX,EDX".Split(mComma);
    public static readonly string[] RegistersIdx = "ESI,EDI,ESP,EBP".Split(mComma);
    public static readonly string[] RegisterPatterns = "_REG,_REG8,_REG16,_REG32,_REGIDX".Split(mComma);

    static Parser() {
      var xRegisters = new List<string>();
      xRegisters.AddRange(Registers8);
      xRegisters.AddRange(Registers16);
      xRegisters.AddRange(Registers32);
      xRegisters.AddRange(RegistersIdx);
      Registers = xRegisters.ToArray();
    }

    protected void NewToken(TokenList aList, ref int rPos) {
      #region Pattern Notes
      // All patterns start with _, this makes them reserved. User can use too, but at own risk of conflict.
      //
      // Wildcards
      // -_REG or ??X
      // -_REG8 or ?H,?L
      // -_REG16 or ?X
      // -_REG32 or E?X
      //     - ? based ones are ugly and less clear
      // -_Keyword
      // -_ABC
      //
      //
      // Multiple Options (All caps only) - Registers only
      // Used to suport EAX,EBX - ie lists. But found out wasnt really needed. May add again later.
      //
      // -AX/AL - Conflict if we ever use /
      // -AX|AL - Conflict if we ever use |
      // -AX,AL - , is unlikely to ever be used as an operator and is logical as a separator. Method calls might use, but likely better to use a space 
      //          since we will only allow simple arguments, not compound.
      // -_REG:AX|AL - End terminator issue
      // -_REG[AX|AL] - Conflict with existing indirect access. Is indirect access always numeric? I think x86 has some register based ones too.
      //
      //
      // Specific: Register, Keyword, AlphaNum
      // -EAX
      #endregion

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

        } else if (char.IsDigit(xChar1)) {
          xToken.Type = TokenType.ValueInt;
        } else if (xChar1 == '$') {
          xToken.Type = TokenType.ValueInt;
          xString = "0x" + xString.Substring(2);

        } else if (IsAlphaNum(xChar1)) { // This must be after check for ValueInt
          string xUpper = xString.ToUpper();

          if (mAllowPatterns) {
            if (RegisterPatterns.Contains(xUpper)) {
              xToken.Type = TokenType.Register;
            } else if (xUpper == "_KEYWORD") {
              xToken.Type = TokenType.Keyword;
              xString = null;
            } else if (xUpper == "_ABC") {
              xToken.Type = TokenType.AlphaNum;
              xString = null;
            }
          }

          if (xToken.Type == TokenType.Unknown) {
            if (Registers.Contains(xUpper)) {
              xToken.Type = TokenType.Register;
            } else if (mKeywords.Contains(xUpper)) {
              xToken.Type = TokenType.Keyword;
            } else {
              xToken.Type = TokenType.AlphaNum;
            }
          }

        } else {
          #region Symbols
          if (xString == "[") {
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
            xToken.Type = TokenType.Equals;
          } else if (xString == ":") {
            xToken.Type = TokenType.Colon;
          } else if (xString == ",") {
            xToken.Type = TokenType.Comma;
          } else if (xString == "<") {
            xToken.Type = TokenType.LessThan;
          } else if (xString == ">") {
            xToken.Type = TokenType.GreaterThan;
          } else if (xString == "?") {
            xToken.Type = TokenType.Question;
          } else if (xString == "@") {
            xToken.Type = TokenType.At;
          }
          #endregion
        }
      }

      xToken.Value = xString;
      xToken.SrcPosStart = mStart;
      xToken.SrcPosEnd = rPos - 1;
      if (mAllWhitespace && xToken.Type != TokenType.WhiteSpace) {
        mAllWhitespace = false;
      }
      mStart = rPos;

      if (mIncludeWhiteSpace || xToken.Type != TokenType.WhiteSpace) {
        aList.Add(xToken);
      }
    }

    protected enum CharType { WhiteSpace, Identifier, Symbol };
    protected void Parse() {
      mTokens = ParseText();
    }

    protected bool IsAlphaNum(char aChar) {
      return char.IsLetterOrDigit(aChar) || aChar == '_' || aChar == '.' || aChar == '$';
    }

    // Initial Parse to convert text to tokens
    protected TokenList ParseText() {
      // Save in comment, might be useful in future. Already had to dig it out of TFS once
      //var xRegex = new Regex(@"(\W)");

      var xResult = new TokenList();
      char xLastChar = ' ';
      CharType xLastCharType = CharType.WhiteSpace;
      char xChar;
      CharType xCharType = CharType.WhiteSpace;
      int i = 0;
      for (i = 0; i < mData.Length; i++) {
        xChar = mData[i];
        if (char.IsWhiteSpace(xChar)) {
          xCharType = CharType.WhiteSpace;
        } else if (IsAlphaNum(xChar)) {
          // _ and . were never likely to stand on their own. ie ESP _ 2 and ESP . 2 are never likely to be used.
          // Having them on their own required a lot of code
          // to treat them as a single unit where we did use them. So we treat them as AlphaNum.
          xCharType = CharType.Identifier;
        } else {
          xCharType = CharType.Symbol;
        }

        // i > 0 - Never do NewToken on first char. i = 0 is just a pass to get char and set lastchar.
        // But its faster as the second short circuit rather than a separate if.
        if (xCharType != xLastCharType && i > 0) {
          NewToken(xResult, ref i);
        }

        xLastChar = xChar;
        xLastCharType = xCharType;
      }

      // Last token
      if (mStart < mData.Length) {
        NewToken(xResult, ref i);
      }

      return xResult;
    }

    public Parser(string aData, bool aIncludeWhiteSpace, bool aAllowPatterns) {
      mData = aData;
      mIncludeWhiteSpace = aIncludeWhiteSpace;
      mAllowPatterns = aAllowPatterns;
      mAllWhitespace = true;

      Parse();
    }
  }
}
