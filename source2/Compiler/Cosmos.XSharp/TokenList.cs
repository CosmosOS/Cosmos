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

      if (Count != aObj.Count) {
        return false;
      }

      for (int i = 0; i < aObj.Count; i++) {
        var xThis = this[i];
        var xThat = aObj[i];
        if (xThis.Type != xThat.Type) {
          return false;
        } else if (xThis.Type == TokenType.AlphaNum || xThat.Type == TokenType.Keyword) {
          if (xThis.Value == null || aObj[i].Value == null) {
          } else if (string.Compare(xThis.Value, xThat.Value, true) != 0) {
            return false;
          }
        } else if (xThis.Type == TokenType.Register) {
          string xThisUpper = xThis.Value.ToUpper();
          string xThatUpper = xThat.Value.ToUpper();

          if (xThisUpper == "_REG" || xThatUpper == "_REG") {
            // true, ie continue

          } else if (xThisUpper == "_REG8" && xThatUpper == "_REG8") {
            return false;
          } else if (xThisUpper == "_REG8" && Parser.Registers8.Contains(xThatUpper)) {
            return false;
          } else if (xThatUpper == "_REG8" && Parser.Registers8.Contains(xThisUpper)) {
            return false;

          } else if (xThisUpper == "_REG16" && xThatUpper == "_REG16") {
            return false;
          } else if (xThisUpper == "_REG16" && Parser.Registers8.Contains(xThatUpper)) {
            return false;
          } else if (xThatUpper == "_REG16" && Parser.Registers8.Contains(xThisUpper)) {
            return false;

          } else if (xThisUpper == "_REG32" && xThatUpper == "_REG32") {
            return false;
          } else if (xThisUpper == "_REG32" && Parser.Registers8.Contains(xThatUpper)) {
            return false;
          } else if (xThatUpper == "_REG32" && Parser.Registers8.Contains(xThisUpper)) {
            return false;

          } else if (xThisUpper != xThatUpper) {
            // Must be last, after patterns
            return false;
          }
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
