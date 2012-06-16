using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Cosmos.Compiler.XSharp {
  public class TokenPattern {
    protected TokenList mTokens;

    public TokenPattern(string aTokenPattern) {
      // Save in comment, might be useful in future. Already had to dig it out of TFS once
      //var xRegex = new Regex(@"(\W)");

      var xParser = new Parser(aTokenPattern, false);
      mTokens = xParser.Tokens;
    }

    public TokenPattern(TokenList aTokens) {
      mTokens = aTokens;
    }

    public static bool operator ==(TokenPattern a1, string a2) {
      var xParse = new Parser(a2, false);
      return a1.Equals(new TokenPattern(xParse.Tokens));
    }
    public static bool operator !=(TokenPattern a1, string a2) {
      var xParse = new Parser(a2, false);
      return !a1.Equals(new TokenPattern(xParse.Tokens));
    }

    public override bool Equals(object aObj) {
      if (aObj is TokenPattern) {
        var xObj = (TokenPattern)aObj;
        if (mTokens.Count == xObj.mTokens.Count) {
          for (int i = 0; i < xObj.mTokens.Count; i++) {
            if (mTokens[i].Type != xObj.mTokens[i].Type) {
              return false;
            }
          }
          return true;
        }
      }
      return false;
    }

    public override int GetHashCode() {
      int xResult = 0;
      var xBytes = new byte[4];
      for (int i = 0; i < mTokens.Count; i = i + 4) {
        for (int j = 0; j < 4; j++) {
          if (j < mTokens.Count) {
            xBytes[j] = (byte)mTokens[i].Type;
          } else {
            xBytes[j] = 0;
          }
        }
        xResult = xResult ^ BitConverter.ToInt32(xBytes, 0);
      }
      //foreach (var x in mTokens) {
      //  xResult = xResult ^ x.Value.GetHashCode();
      //}
      return xResult;
    }

  }
}
