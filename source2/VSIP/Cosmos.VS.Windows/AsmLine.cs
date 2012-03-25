using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.VS.Windows {
  public class AsmLine {
    protected string mText;
    public override string ToString() {
      return mText;
    }

    public AsmLine(string aText) {
      mText = aText;
    }
  }

}
