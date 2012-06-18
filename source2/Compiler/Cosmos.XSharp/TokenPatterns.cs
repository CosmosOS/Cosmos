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

    protected void AddPatterns() {
      AddPattern("! Move EAX, 0",
        "new LiteralAssemblerCode(\"{0}\");"
      );
      AddPattern("# Comment",
        "new Comment(\"{0}\");"
      );
      AddPattern("_ABC:" ,
        "new Label(\"{0}\");"
      );

      AddPattern("_REG = 123",
        "new Mov{{ DestinationReg = RegistersEnum.{0}, SourceValue = {2} }};"
      );
      AddPattern("_REG = _REG",
        "new Mov{{ DestinationReg = RegistersEnum.{0}, SourceReg = RegistersEnum.{2} }};"
      );
      AddPattern("_REG = _REG[1]",
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

      AddPattern("Port[DX] = AX", 
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

      AddPattern("Call _ABC", delegate(TokenList aTokens, ref List<string> rCode) {
        string xLabel = aTokens[1].Value;
        rCode.Add("new Call {{ DestinationLabel = " + Quoted(mGroup + "_" + xLabel) + " }};");
      });

      AddPattern("Exit", delegate(TokenList aTokens, ref List<string> rCode) {
        rCode.Add("new Jump {{ DestinationLabel = " + Quoted(mGroup + "_" + mProcedureName + "_Exit") + " }};");
      });

      AddPattern("InterruptHandler _ABC {", delegate(TokenList aTokens, ref List<string> rCode) {
        mInIntHandler = true;
        mProcedureName = aTokens[1].Value;
        rCode.Add("new Label(\"" + mGroup + "_{1}\");");
      });

      AddPattern("Jump _ABC", delegate(TokenList aTokens, ref List<string> rCode) {
        rCode.Add("new Jump {{ DestinationLabel = \"" + mGroup + "_{1}\" }};");
      });

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
      CodeFunc xAction = null;
      List<string> xResult = new List<string>();

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
        throw new Exception("Token pattern not found.");
      }

      xPattern.Code(xTokens, ref xResult); 

      for(int i = 0; i < xResult.Count; i++) {
        xResult[i] = string.Format(xResult[i], xTokens.Select(c => c.Value).ToArray());
      }
      return xResult;
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
