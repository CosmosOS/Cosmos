namespace Cosmos.Assembler {
  public class Label : Instruction {
      public string Comment
      {
          get
          {
              return mComment;
          }
      }

      private readonly string mComment;

      public Label(string aName, bool isGlobal = false)
      : this(aName, string.Empty)
    {
        mIsGlobal = isGlobal;
    }

    public Label(string aName, string aComment) {
      mName = aName;
      if (aName.StartsWith(".")) {
        mQualifiedName = LastFullLabel + aName;
      } else {
        mQualifiedName = aName;
        // Some older code passes the whole label in the argument, so we check for any . in it.
        // That assumes that the main prefix can never have a . in it.
        // This code isnt perfect and doenst label X# code properly, but we don't care about
        // auto emitted X# labels for now.
        var xParts = aName.Split('.');
        if (xParts.Length < 3) {
          LastFullLabel = aName;
        }
      }
      mComment = aComment;
    }

    public static string GetLabel(object aObject) {
      Label xLabel = aObject as Label;
      if (xLabel == null) {
        return string.Empty;
      }
      return xLabel.Name;
    }

    public static string LastFullLabel { get; set; }

    public string QualifiedName {
        get
        {
            return mQualifiedName;
        }
    }

      private readonly string mQualifiedName;

    public bool IsGlobal {
        get
        {
            return mIsGlobal;
        }
    }

      private readonly bool mIsGlobal;

    private readonly string mName;
    public string Name {
      get { return mName; }
    }

      public override void WriteText(Cosmos.Assembler.Assembler aAssembler, System.IO.TextWriter aOutput)
      {
          if (IsGlobal)
          {
              aOutput.Write("global ");
              aOutput.WriteLine(QualifiedName);
          }
          aOutput.Write(QualifiedName);
          aOutput.Write(":");
          if (Comment.Length > 0)
          {
              aOutput.Write(" ;");
              aOutput.Write(Comment);
          }
      }

      public override bool IsComplete(Assembler aAssembler) {
      return true;
    }

    public override void WriteData(Cosmos.Assembler.Assembler aAssembler, System.IO.Stream aOutput) { }
  }
}