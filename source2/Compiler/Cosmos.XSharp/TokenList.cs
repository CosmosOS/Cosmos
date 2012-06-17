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
      var xParser = new Parser(aPattern, false, true);
      return PatternMatches(xParser.Tokens);
    }

    public bool PatternMatches(TokenList aObj) {
      // Dont compare TokenHashCodes, they take just as long to calculate
      // as a full comparison. Besides this function is often called after
      // comparing hash codes already.
      //
      // For mathching, we assume that only the "this" side can have wildcards or lists
      // and that the aObj side will have only exacts. This might need to change
      // in the future, but will complicate the code a bit. Especially if both sides
      // could have wildcards (Doesn't even make sense at this point)

      if (Count != aObj.Count) {
        return false;
      }

      for (int i = 0; i < aObj.Count; i++) {
        var xThis = this[i];
        if (xThis.Type != aObj[i].Type) {
          return false;
        } else if (xThis.Type == TokenType.AlphaNum || xThis.Type == TokenType.Keyword) {
          if (xThis.Value != null && string.Compare(xThis.Value, aObj[i].Value, true) != 0) {
            //return false;
          }
        } else if (xThis.Type == TokenType.Register) {
        }
      }
     
      return true;
    }

    // We could use values to further differntiate, however
    // with types alone it still provides a decent and fash hash.
    public override int GetHashCode() {
      return GetPatternHashCode();
    }
  }
}
