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
      var xRegex = new Regex(@"(\W)");
      var xParts = xRegex.Split(aTokenPattern);
      var xTokenTypes = new List<TokenType>();

      foreach (string xPart in xParts) {
        if (string.IsNullOrWhiteSpace(xPart)) {
          continue;
        }

        TokenType xTokenType;
        if (string.Compare(xPart, "REG") == 0) {
          xTokenType = TokenType.Register;
        } else if (string.Compare(xPart, "ABC") == 0) {
          xTokenType = TokenType.AlphaNum;
        } else if (char.IsDigit(xPart[0])) {
          xTokenType = TokenType.ValueInt;
        } else {
          xTokenType = Token.GetTypeForSymbol(xPart);
          if (xTokenType == TokenType.Unknown) {
            throw new Exception("Unrecognized string token: " + xPart);
          }
        }
        xTokenTypes.Add(xTokenType);
      }

      mTokenTypes = xTokenTypes.ToArray();
      Init();
    }

    public TokenPattern(TokenType[] aTokenTypes) {
      // We dont copy the array, so technically it is mutable but in our usage
      // constant arrays are passed in.
      mTokenTypes = aTokenTypes;
      Init();
    }

    protected void Init() {
      mHashCode = CalcHashCode();
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
