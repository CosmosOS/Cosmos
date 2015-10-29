using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.VS.Windows {
  public abstract class AsmLine {
  }

  public class AsmCode : AsmLine {
    protected string mText = "";
    public string Text {
      get { return mText; }
      set { 
        mText = value;
        mIsDebugCode = mText.Trim().ToUpper().EndsWith("INT3");
      }
    }

    public override string ToString() {
      return mText;
    }

    protected AsmLabel mAsmLabel;
    public AsmLabel AsmLabel {
      get { return mAsmLabel; }
      set { mAsmLabel = value; }
    }

    protected bool mIsDebugCode;
    public bool IsDebugCode {
      get { return mIsDebugCode; }
    }

    public bool LabelMatches(string aLabel) {
      return (AsmLabel != null && AsmLabel.Label == aLabel);
    }

    public AsmCode(string aText) {
      // Use prop setter as it has checks
      Text = aText;
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
