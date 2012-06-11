using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.Compiler.XSharp {
  public class Parser {
    int mStart = 0;
    protected string mData;
    protected TokenType[] mPattern;

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
      "EDX", "EX", "DH", "DL",
    };

    public bool PatternMatches(TokenType[] aPattern) {
      // SequenceEqual is slow with arrays
      //return mPattern.SequenceEqual(aPattern);
      //TODO: If always 8 or less can use int64[]
      // Could also use string arrays
      // or maybe tree

    	if (mPattern.Length == aPattern.Length) {
	      for (int i = 0; i < aPattern.Length; i++) {
          if (mPattern[i] != aPattern[i]) {
		        return false;
		      }
	      }
	      return true;
	    }
	    return false;
    } 

    protected void NewToken(ref int rPos) {
      string xString = null;
      char xChar1 = mData[mStart];
      var xToken = new Token();
      if ((mTokens.Count == 0) && "#!".Contains(xChar1)) {
        rPos = mData.Length; // This will account for the dummy whitespace at the end.
        xString = mData.Substring(mStart, rPos - mStart);
        if (xChar1 == '#') {
          xToken.Type = TokenType.Comment;
        } else if (xChar1 == '!') {
          xToken.Type = TokenType.Literal;
        }
      } else {
        xString = mData.Substring(mStart, rPos - mStart);

        if (xString == "++") {
          xToken.Type = TokenType.Inc;
        } else if (xString == "--") {
          xToken.Type = TokenType.Dec;
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
        } else if (char.IsLetter(xChar1)) {
          if (xString.EndsWith(":")) {
            xToken.Type = TokenType.Label;
          } else {
            string xUpper = xString.ToUpper();
            if (mRegisters.Contains(xUpper)) {
              xToken.Type = TokenType.Register;
            } else if (mOps.Contains(xUpper)) {
              xToken.Type = TokenType.Op;
            } else {
              throw new Exception("Unrecognized text.");
            }
          }
        } else if (char.IsDigit(xChar1)) {
          xToken.Type = TokenType.ValueNumber;
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
        // : is for labels
        bool xIsLetterOrDigit = char.IsLetterOrDigit(xChar) || xChar == ':';
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

      mPattern = mTokens.Select(c => c.Type).ToArray();
    }

    public Parser(string aData) {
      // We add a dummy whitespace to force the parser to trigger a NewToken for the last item.
      mData = aData + ' ';
      Parse();
    }
  }
}
