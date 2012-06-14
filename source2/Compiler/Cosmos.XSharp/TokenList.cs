using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Cosmos.Compiler.XSharp {
  public class TokenList : ReadOnlyCollection<Token> {
    public TokenList(List<Token> aList)
      : base(aList) {
      Pattern = new TokenPattern(this);
    }

    public readonly TokenPattern Pattern;
  }
}
