using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.Compiler.XSharp {
  public class TokenPattern {
    TokenType[] mTokenTypes;
    // Since we make our contents immutable, we can cache the hash code
    readonly int mHashCode;

    public TokenPattern(TokenType[] aTokenTypes) {
      // We dont copy the array, so technically it is mutable but in our usage
      // constant arrays are passed in.
      mTokenTypes = aTokenTypes;
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
