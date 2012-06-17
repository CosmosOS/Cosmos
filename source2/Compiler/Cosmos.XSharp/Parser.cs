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

    public static string[] mKeywords = new string[] { 
      "CALL"
      , "END", "EXIT"
      , "GROUP"
      , "INTERRUPTHANDLER"
      , "JUMP"
      , "POPALL", "PUSHALL", "PROCEDURE", "PORT"
      , "RETURN", "RETURNINTERRUPT"
    };
    public static readonly string RegisterList;
    public static readonly string[] Registers;
    public static readonly string Register8List;
    public static readonly string[] Registers8;
    public static readonly string Register16List;
    public static readonly string[] Registers16;
    public static readonly string Register32List;
    public static readonly string[] Registers32;
    public static readonly string RegisterIdxList;
    public static readonly string[] RegistersIdx;

    static Parser() {
      // These do not work when initialized inline, despite it compiling.
      // So they are in a static ctor instead.
      var xComma = ",".ToCharArray();
      Register8List = "AH,AL,BH,BL,CH,CL,DH,DL";
      Registers8 = Register8List.Split(xComma);
      Register16List = "AX,BX,CX,DX";
      Registers16 = Register16List.Split(xComma);
      Register32List = "EAX,EBX,ECX,EDX";
      Registers32 = Register32List.Split(xComma);
      RegisterIdxList = "ESI,EDI,ESP,EBP";
      RegistersIdx = RegisterIdxList.Split(xComma);
      RegisterList = Register8List + "," + Register16List + "," + Register32List + "," + RegisterIdxList;
      Registers = RegisterList.Split(xComma);
    }

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
          if (Registers.Contains(xUpper)) {
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
        } else if (xString == ",") {
          xToken.Type = TokenType.Comma;
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
      mTokens = ParseTokens(xTokens);
    }

    // Rescan token patterns
    protected TokenList ParseTokens(List<Token> aTokens) {
      var xResult = new TokenList();

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
      // Save in comment, might be useful in future. Already had to dig it out of TFS once
      //var xRegex = new Regex(@"(\W)");

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
