using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Cosmos.Compiler.XSharp {
  public class TokenPatterns {
    protected class Pattern {
      public TokenList Tokens;
      public int Hash;
      public CodeFunc Code;
    }

    public bool EmitUserComments = true;
    public delegate void CodeFunc(TokenList aTokens, ref List<string> rCode);
    protected List<Pattern> mPatterns = new List<Pattern>();
    protected string mGroup;
    protected string mProcedureName;
    protected bool mInIntHandler;

    public TokenPatterns() {
      AddPatterns();
    }

    protected string Quoted(string aString) {
      return "\"" + aString + "\"";
    }

    protected int IntValue(Token aToken) {
      if (aToken.Value.StartsWith("0x")) {
        return int.Parse(aToken.Value.Substring(2), NumberStyles.AllowHexSpecifier);
      } else {
        return int.Parse(aToken.Value);
      }
    }

    protected string GroupLabel(int aIndex) {
      return GroupLabel("{" + aIndex + "}");
    }
    protected string GroupLabel(string aLabel) {
      return mGroup + "_" + aLabel;
    }

    protected string ProcLabel(int aIndex) {
      return ProcLabel("{" + aIndex + "}");
    }
    protected string ProcLabel(string aLabel) {
      return mGroup + "_" + mProcedureName + "_" + aLabel;
    }

    protected string GetLabel(TokenList aTokens, int aIndex) {
      if (aTokens[aIndex].Type == TokenType.AlphaNum) {
        return ProcLabel(aIndex);
      } else if (aTokens[aIndex + 1].Type == TokenType.Dot) {
        return aTokens[aIndex + 2].Value;
      } else {
        return GroupLabel(aIndex + 1);
      }
    }

    protected void AddPatterns() {
      AddPattern("! Move EAX, 0",
        "new LiteralAssemblerCode(\"{0}\");"
      );

      AddPattern("# Comment", delegate(TokenList aTokens, ref List<string> rCode) {
        if (EmitUserComments) {
          rCode.Add("new Comment(\"{0}\");");
        }
      });

      // Labels
      // Local and proc level are used most, so designed to make their syntax shortest.
      // Think of the dots like a directory, . is current group, .. is above that.
      // ..Name: - Global level. Emitted exactly as is.
      // .Name: - Group level. Group_Name
      // Name: - Procedure level. Group_ProcName_Name
      AddPattern(new string[] { ".._ABC:", "._ABC:", "_ABC:" },
        delegate(TokenList aTokens, ref List<string> rCode) {
          rCode.Add("new Label(" + Quoted(GetLabel(aTokens, 0)) + ");");
        }
      );

      AddPattern(new string[] { "Call .._ABC", "Call ._ABC", "Call _ABC" },
        delegate(TokenList aTokens, ref List<string> rCode) {
          rCode.Add("new Call {{ DestinationLabel = " + Quoted(GetLabel(aTokens, 1)) + " }};");
        }
      );

      // TODO - Combine these to code to scan
      AddPattern("_REG < 123",
        "new Compare {{ DestinationReg = RegistersEnum.{0}, SourceValue = {2} }};"
      );

      // TODO - Combine these to code to scan
      #region _REG = ...
      AddPattern("_REG = 123",
        "new Mov{{ DestinationReg = RegistersEnum.{0}, SourceValue = {2} }};"
      );
      
      AddPattern("_REG = _REG",
        "new Mov{{ DestinationReg = RegistersEnum.{0}, SourceReg = RegistersEnum.{2} }};"
      );
      
      AddPattern("_REG = _REG32[1]",
        "new Mov {{"
          + " DestinationReg = RegistersEnum.{0}"
          + ", SourceReg = RegistersEnum.{2}, SourceIsIndirect = true, SourceDisplacement = {4}"
          + "}};"
      );
      
      AddPattern("_REG = _REG[-1]",
        "new Mov {{"
          + " DestinationReg = RegistersEnum.{0}"
          + ", SourceReg = RegistersEnum.{2}, SourceIsIndirect = true, SourceDisplacement = -{4}"
          + "}};"
      );

      AddPattern("_REG32[0] = _REG",
        "new Mov {{"
          + " DestinationReg = RegistersEnum.{0}, DestinationIsIndirect = true, DestinationDisplacement = {2}"
          + ", SourceReg = RegistersEnum.{5}"
          + "}};"
      );
      #endregion

      AddPattern("Port[DX] = AL", 
        // TODO: DX only for index
        // TODO: Src _REG can only be EAX, AX, AL
        "new Out {{ DestinationReg = RegistersEnum.{5}}};"
      );

      AddPattern("+_REG",
        "new Push {{"
          + " DestinationReg = RegistersEnum.{1}"
          + "}};"
      );
      AddPattern("-_REG",
        "new Pop {{"
          + " DestinationReg = RegistersEnum.{1}"
          + "}};"
      );

      AddPattern("_ABC = _REG", delegate(TokenList aTokens, ref List<string> rCode) {
        rCode.Add("new Mov {{"
         + " DestinationRef = Cosmos.Assembler.ElementReference.New(\"" + mGroup + "_{0}\"), DestinationIsIndirect = true"
         + " , SourceReg = RegistersEnum.{2}"
         + " }};");
      });

      // TODO: Allow asm to optimize these to Inc/Dec
      AddPattern("_REG + 1", delegate(TokenList aTokens, ref List<string> rCode) {
        if (IntValue(aTokens[2]) == 1) {
          rCode.Add("new INC {{ DestinationReg = RegistersEnum.{0} }};");
        } else {
          rCode.Add("new Add {{ DestinationReg = RegistersEnum.{0}, SourceValue = {2} }};");
        }
      });

      AddPattern("_REG - 1", delegate(TokenList aTokens, ref List<string> rCode) {
        if (IntValue(aTokens[2]) == 1) {
          rCode.Add("new Dec {{ DestinationReg = RegistersEnum.{0} }};");
        } else {
          rCode.Add("new Sub {{ DestinationReg = RegistersEnum.{0}, SourceValue = {2} }};");
        }
      });

      AddPattern("}", delegate(TokenList aTokens, ref List<string> rCode) {
        rCode.Add("new Label(\"" + mGroup + "_" + mProcedureName + "_Exit\");");
        if (mInIntHandler) {
          rCode.Add("new IRET();");
        } else {
          rCode.Add("new Return();");
        }
      });

      AddPattern("Group _ABC", delegate(TokenList aTokens, ref List<string> rCode) {
        mGroup = aTokens[1].Value;
      });

      AddPattern("Exit", delegate(TokenList aTokens, ref List<string> rCode) {
        rCode.Add("new Jump {{ DestinationLabel = " + Quoted(ProcLabel("Exit")) + " }};");
      });

      AddPattern("InterruptHandler _ABC {", delegate(TokenList aTokens, ref List<string> rCode) {
        mInIntHandler = true;
        mProcedureName = aTokens[1].Value;
        rCode.Add("new Label(\"" + mGroup + "_{1}\");");
      });

      AddPattern("Jump _ABC", 
        delegate(TokenList aTokens, ref List<string> rCode) {
          rCode.Add("new Jump {{ DestinationLabel = \"" + mGroup + "_{1}\" }};");
        }
      );

      AddPattern("Return", "new Return();");
      AddPattern("ReturnInterrupt", "new IRET();");
      AddPattern("PopAll", "new Popad();");
      AddPattern("PushAll", "new Pushad();");

      AddPattern("Procedure _ABC {", delegate(TokenList aTokens, ref List<string> rCode) {
        mInIntHandler = false;
        mProcedureName = aTokens[1].Value;
        rCode.Add("new Label(\"" + mGroup + "_{1}\");");
      });
    }

    public List<string> GetCode(string aLine) {
      var xParser = new Parser(aLine, false, false);
      var xTokens = xParser.Tokens;
      var xResult = new List<string>();
      int xHash = xTokens.GetPatternHashCode();

      // Get a list of matching hashes, but then we have to 
      // search for exact pattern match because it is possible
      // to have duplicate hashes. Hashes just provide us a quick way
      // to reduce the search.
      var xPatterns = mPatterns.Where(q => q.Hash == xHash);
      Pattern xPattern = null;
      foreach (var x in xPatterns) {
        if (x.Tokens.PatternMatches(xTokens)) {
          xPattern = x;
          break;
        }
      }
      if (xPattern == null) {
        throw new Exception("Token pattern not found: " + aLine);
      }

      xPattern.Code(xTokens, ref xResult); 

      for(int i = 0; i < xResult.Count; i++) {
        xResult[i] = string.Format(xResult[i], xTokens.Select(c => c.Value).ToArray());
      }
      return xResult;
    }

    protected void AddPattern(string[] aPatterns, CodeFunc aCode) {
      foreach (var xPattern in aPatterns) {
        AddPattern(xPattern, aCode);
      }
    }
    protected void AddPattern(string aPattern, CodeFunc aCode) {
      var xParser = new Parser(aPattern, false, true);
      var xPattern = new Pattern() {
        Tokens = xParser.Tokens,
        Hash = xParser.Tokens.GetHashCode(),
        Code = aCode
      };

      mPatterns.Add(xPattern);
    }
    protected void AddPattern(string aPattern, string aCode) {
      AddPattern(aPattern, delegate(TokenList aTokens, ref List<string> rCode) {
        rCode.Add(aCode);
      });
    }

  }
}
