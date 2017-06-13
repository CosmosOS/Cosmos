using System;
using System.Collections.Generic;

namespace XSharp.Common {
  public class TokenList : List<Token> {

    public new Token this[int aIdx] {
      get {
        if (aIdx < 0) {
          aIdx = Count + aIdx;
        }
        return base[aIdx];
      }
      set {
        if (aIdx < 0) {
          aIdx = Count + aIdx;
        }
        base[aIdx] = value;
      }
    }

    public Token Last() {
      return base[Count - 1];
    }

    public int GetPatternHashCode() {
      int xResult = 0;
      var xBytes = new byte[4];
      for (int i = 0; i < Count; i = i + 4) {
        for (int j = 0; j < 4; j++) {
          if (j < Count) {
            xBytes[j] = (byte)base[i].Type;
          } else {
            xBytes[j] = 0;
          }
        }
        xResult = xResult ^ BitConverter.ToInt32(xBytes, 0);
      }
      xResult = xResult ^ Count;
      return xResult;
    }

    protected bool RegistersMatch(string aThisUpper, string aThatUpper, string aPattern, IDictionary<string, XSRegisters.Register> aRegisters) {
      // ONLY check if its our pattern. We need to return true to continue other pattern checks
      // if not current pattern.
      if (aThisUpper == aPattern || aThatUpper == aPattern) {
        if (aRegisters.ContainsKey(aThatUpper) || aRegisters.ContainsKey(aThisUpper)) {
          return true;
        }
        return false;
      }
      return true;
    }

    // BlueSkeye : Seems to be unused. Commented out.
    //public bool PatternMatches(string aPattern) {
    //  var xParser = new Parser(aPattern, false, true);
    //  return PatternMatches(xParser.Tokens);
    //}

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
        } else if (xThis.Type == TokenType.AlphaNum || xThis.Type == TokenType.Keyword || xThis.Type == TokenType.Operator || xThis.Type == TokenType.Delimiter) {
          if (xThis.RawValue == null || aObj[i].RawValue == null) {
          } else if (string.Compare(xThis.RawValue, xThat.RawValue, true) != 0) {
            return false;
          }
        } else if (xThis.Type == TokenType.Register) {
          string xThisUpper = xThis.RawValue.ToUpper();
          string xThatUpper = xThat.RawValue.ToUpper();

          if (xThisUpper == "_REG" || xThatUpper == "_REG") {
            // true, ie continue
          } else if (RegistersMatch(xThisUpper, xThatUpper, "_REG8", Parser.Registers8)) {
          } else if (RegistersMatch(xThisUpper, xThatUpper, "_REG16", Parser.Registers16)) {
          } else if (RegistersMatch(xThisUpper, xThatUpper, "_REG32", Parser.Registers32)) {
          } else if (RegistersMatch(xThisUpper, xThatUpper, "_REGIDX", Parser.RegistersIdx)) {
          } else if (RegistersMatch(xThisUpper, xThatUpper, "_REGADDR", Parser.RegistersAddr)) {
          } else if (xThisUpper == xThatUpper) {
            // This covers _REG==_REG, _REG8==_REG8, ... and DX==DX
            // Must be last, after patterns
          } else {
            return false;
          }
        }
      }

      return true;
    }

    // BlueSkeye : Seems to be unused. Commented out.
    //public int IndexOf(string aValue) {
    //  for (int i = 0; i < Count; i++) {
    //    if (this[i].Value == aValue) {
    //      return i;
    //    }
    //  }
    //  return -1;
    //}

    // We could use values to further differntiate, however
    // with types alone it still provides a decent and fash hash.
    public override int GetHashCode() {
      return GetPatternHashCode();
    }
  }
}
