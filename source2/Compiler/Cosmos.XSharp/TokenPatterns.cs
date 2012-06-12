using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.Compiler.XSharp {
  public class TokenPatterns : Dictionary<TokenPattern, string> {
    public void Add(TokenType[] aTokenTypes, string aCode) {
      Add(new TokenPattern(aTokenTypes), aCode);
    }
  }
}
