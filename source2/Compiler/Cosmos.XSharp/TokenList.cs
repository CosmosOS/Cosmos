using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Cosmos.Compiler.XSharp {
  public class TokenList : List<Token> {

    public int GetPatternHashCode() {
      int xResult = 0;
      var xBytes = new byte[4];
      for (int i = 0; i < Count; i = i + 4) {
        for (int j = 0; j < 4; j++) {
          if (j < Count) {
            xBytes[j] = (byte)this[i].Type;
          } else {
            xBytes[j] = 0;
          }
        }
        xResult = xResult ^ BitConverter.ToInt32(xBytes, 0);
      }
      xResult = xResult ^ Count;
      return xResult;
    }

    public bool PatternMatches(string aPattern) {
      var xParser = new Parser(aPattern, false);
      return PatternMatches(xParser.Tokens);
    }

    public bool PatternMatches(TokenList aObj) {
      // Dont compare TokenHashCodes, they take just as long to calculate
      // as a full comparison.
      if (Count == aObj.Count) {
        for (int i = 0; i < aObj.Count; i++) {
          if (this[i].Type != aObj[i].Type) {
            return false;
          }
        }
        return true;
      }
      return false;
    }

    // We could use values to further differntiate, however
    // with types alone it still provides a decent and fash hash.
    public override int GetHashCode() {
      return GetPatternHashCode();
    }
  }
}
