using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.VS.Windows {
  public abstract class AsmLine {
  }

  public class AsmCode : AsmLine {
    protected string mText;
    public override string ToString() {
      return mText;
    }

    protected AsmLabel mLabel;
    public AsmLabel Label {
      get { return mLabel; }
      set { mLabel = value; }
    }

    public AsmCode(string aText) {
      mText = aText;
    }
  }

  public class AsmComment : AsmLine {
    protected string mComment;

    public override string ToString() {
      return "; " + mComment;
    }

    public AsmComment(string aComment) {
      mComment = aComment;
    }
  }

  public class AsmLabel : AsmLine {
    protected string mLabel;
    public string Label {
      get { return mLabel; }
    }

    protected string mComment = "";
    public string Comment {
      get { return mComment; }
      set { mComment = value; }
    }

    public override string ToString() {
      string xResult = mLabel + ":";
      if (mLabel.Length > 0) {
        xResult = xResult + " ;" + mComment;
      }
      return xResult;
    }

    public AsmLabel(string aLabel) {
      mLabel = aLabel;
    }
  }

}
