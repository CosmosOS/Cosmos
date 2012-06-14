using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Cosmos.Compiler.XSharp {
  public class TokenPattern {
    protected TokenType[] mTokenTypes;
    // Since we make our contents immutable, we can cache the hash code
    protected int mHashCode;

    public TokenPattern(string aTokenPattern) {
      // Save in comment, might be useful in future. Already had to dig it out of TFS once
      //var xRegex = new Regex(@"(\W)");

      var xParser = new Parser(aTokenPattern, false);
      mTokenTypes = xParser.Tokens.Select(c => c.Type).ToArray();
      Init();
    }

    public TokenPattern(TokenList aTokenTypes) {
      mTokenTypes = aTokenTypes.Select(c => c.Type).ToArray();
      Init();
    }

    public TokenPattern(TokenType[] aTokenTypes) {
      mTokenTypes = (TokenType[])aTokenTypes.Clone();
      Init();
    }

    protected void Init() {
      mHashCode = CalcHashCode();
    }

    public bool Matches(string aPattern) {
      var xParse = new Parser(aPattern, false);
      return Equals(xParse.Tokens.Pattern);
    }

    public override bool Equals(object aObj) {
      if (aObj is TokenPattern) {
        var xObj = (TokenPattern)aObj;
        if (mTokenTypes.Length == xObj.mTokenTypes.Length) {
          for (int i = 0; i < xObj.mTokenTypes.Length; i++) {
            if (mTokenTypes[i] != xObj.mTokenTypes[i]) {
              return false;
            }
          }
          return true;
        }
      }
      return false;
    }

    public override int GetHashCode() {
      return mHashCode;
    }

    protected int CalcHashCode() {
      int xResult = 0;
      var xBytes = new byte[4];
      for (int i = 0; i < mTokenTypes.Length; i = i + 4) {
        for (int j = 0; j < 4; j++) {
          if (j < mTokenTypes.Length) {
            xBytes[j] = (byte)mTokenTypes[i];
          } else {
            xBytes[j] = 0;
          }
        }
        xResult = xResult ^ BitConverter.ToInt32(xBytes, 0);
      }
      return xResult;
    }
  }
}
