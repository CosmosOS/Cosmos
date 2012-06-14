using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.Compiler.XSharp {
  public class TokenList : List<Token> {

    protected TokenPattern mPattern;
    public TokenPattern Pattern {
      get {
        // this is cached... so the user must not change the list after we generate this.
        // TODO: In future lock it down by only allowing add and read this[], but not changes
        if (mPattern == null) {
          mPattern = new TokenPattern(this); 
        }
        return mPattern; 
      }
    }
  }
}
