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
    protected Dictionary<string, CodeFunc> mKeywords = new Dictionary<string, CodeFunc>();
    protected string mGroup;
    protected string mProcedureName;
    protected bool mInIntHandler;

    public TokenPatterns() {
      AddPatterns();
      AddKeywords();
    }

    protected string Quoted(string aString) {
      return "\"" + aString + "\"";
    }

    protected void AddKeywords() {
      AddKeyword("Call", delegate(TokenList aTokens, ref List<string> rCode) {
        string xLabel = aTokens[1].Value;
        if (aTokens.PatternMatches("Call ABC123")) {
          rCode.Add("new Call {{ DestinationLabel = " + Quoted(mGroup + "_" + xLabel) + " }};");
        } else {
          rCode = null;
        }
      });

      AddKeyword("Exit", delegate(TokenList aTokens, ref List<string> rCode) {
        rCode.Add("new Jump {{ DestinationLabel = " + Quoted(mGroup + "_" + mProcedureName + "_Exit") + " }};");
      });

      AddKeyword("Group", delegate(TokenList aTokens, ref List<string> rCode) {
        if (aTokens.PatternMatches("Group ABC123")) {
          mGroup = aTokens[1].Value;
        } else {
          rCode = null;
        }
      });

      AddKeyword("InterruptHandler", delegate(TokenList aTokens, ref List<string> rCode) {
        mInIntHandler = true;
        if (aTokens.PatternMatches("InterruptHandler ABC123 {")) {
          mProcedureName = aTokens[1].Value;
          rCode.Add("new Label(\"" + mGroup + "_{1}\");");
        } else {
          rCode = null;
        }
      });

      AddKeyword("Jump", delegate(TokenList aTokens, ref List<string> rCode) {
        if (aTokens.PatternMatches("Jump ABC123")) {
          rCode.Add("new Jump {{ DestinationLabel = \"" + mGroup + "_{1}\" }};");
        } else {
          rCode = null;
        }
      });

      AddKeyword("Return", "new Return();");
      AddKeyword("ReturnInterrupt", "new IRET();");
      AddKeyword("PopAll", "new Popad();");
      AddKeyword("PushAll", "new Pushad();");

      AddKeyword("Procedure", delegate(TokenList aTokens, ref List<string> rCode) {
        mInIntHandler = false;
        if (aTokens.PatternMatches("Procedure ABC123 {")) {
          mProcedureName = aTokens[1].Value;
          rCode.Add("new Label(\"" + mGroup + "_{1}\");");
        } else {
          rCode = null;
        }
      });
    }

    protected void AddKeyword(string aKeyword, CodeFunc aCode) {
      mKeywords.Add(aKeyword.ToUpper(), aCode);
    }

    protected void AddKeyword(string aKeyword, string aCode) {
      AddKeyword(aKeyword, delegate(TokenList aTokens, ref List<string> rCode) {
        rCode.Add(aCode);
      });
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
      AddPattern("ABC123:" ,
        "new Label(\"{0}\");"
      );

      AddPattern("REG = 123",
        "new Mov{{ DestinationReg = RegistersEnum.{0}, SourceValue = {2} }};"
      );
      AddPattern("REG = REG",
        "new Mov{{ DestinationReg = RegistersEnum.{0}, SourceReg = RegistersEnum.{2} }};"
      );
      AddPattern("REG = REG[1]",
        "new Mov {{"
          + " DestinationReg = RegistersEnum.{0}"
          + ", SourceReg = RegistersEnum.{2}, SourceIsIndirect = true, SourceDisplacement = {4}"
          + "}};"
      );
      AddPattern("REG = REG[-1]",
        "new Mov {{"
          + " DestinationReg = RegistersEnum.{0}"
          + ", SourceReg = RegistersEnum.{2}, SourceIsIndirect = true, SourceDisplacement = -{4}"
          + "}};"
      );

      AddPattern("Port[DX] = AX", 
        // TODO: DX only for index
        // TODO: Src reg can only be EAX, AX, AL
        "new Out {{ DestinationReg = RegistersEnum.{5}}};"
      );

      AddPattern("+REG",
        "new Push {{"
          + " DestinationReg = RegistersEnum.{1}"
          + "}};"
      );
      AddPattern("-REG",
        "new Pop {{"
          + " DestinationReg = RegistersEnum.{1}"
          + "}};"
      );

      AddPattern("ABC123 = REG", delegate(TokenList aTokens, ref List<string> rCode) {
        rCode.Add("new Mov {{"
         + " DestinationRef = Cosmos.Assembler.ElementReference.New(\"" + mGroup + "_{0}\"), DestinationIsIndirect = true"
         + " , SourceReg = RegistersEnum.{2}"
         + " }};");
      });

      // TODO: Allow asm to optimize these to Inc/Dec
      AddPattern("REG + 1", delegate(TokenList aTokens, ref List<string> rCode) {
        if (IntValue(aTokens[2]) == 1) {
          rCode.Add("new INC {{ DestinationReg = RegistersEnum.{0} }};");
        } else {
          rCode.Add("new Add {{ DestinationReg = RegistersEnum.{0}, SourceValue = {2} }};");
        }
      });

      AddPattern("REG - 1", delegate(TokenList aTokens, ref List<string> rCode) {
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
    }

    public List<string> GetCode(string aLine) {
      var xParser = new Parser(aLine, false, false);
      var xTokens = xParser.Tokens;
      CodeFunc xAction = null;
      List<string> xResult = new List<string>();

      if (xTokens[0].Type == TokenType.Keyword) {
        if (mKeywords.TryGetValue(xTokens[0].Value.ToUpper(), out xAction)) {
          xAction(xTokens, ref xResult);
          if (xResult == null) {
            throw new Exception("Unrecognized syntax for keyword: " + xTokens[0].Value);
          }
        }
      }

      if (xAction == null) {
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
      }

      for(int i = 0; i < xResult.Count; i++) {
        xResult[i] = string.Format(xResult[i], xTokens.Select(c => c.Value).ToArray());
      }
      return xResult;
    }

    protected TokenList ParsePatterns(TokenList aTokens) {
      // Wildcards (All caps only)
      // -REG or ??X
      // -REG8 or ?H,?L
      // -REG16 or ?X
      // -REG32 or E?X
      //     - ? based ones are ugly and less clear
      // -KEYWORD
      // -ABC123
      //
      // Multiple Options (All caps only) - Registers only
      // -AX/AL - Conflict if we ever use /
      // -AX|AL - Conflict if we ever use |
      // -AX,AL - , is unlikely to ever be used as an operator and is logical as a separator. Method calls might use, but likely better to use a space 
      //          since we will only allow simple arguments, not compound.
      // -REG:AX|AL - End terminator issue
      // -REG[AX|AL] - Conflict with existing indirect access. Is indirect access always numeric? I think x86 has some register based ones too.
      //
      // Specific: Register, Keyword, AlphaNum
      // -EAX
      var xResult = new TokenList();
      Token xNext;
      int xCount = aTokens.Count;
      for (int i = 0; i < xCount; i++) {
        var xToken = aTokens[i];
        xNext = null;
        if (i + 1 < xCount) {
          xNext = aTokens[i + 1];
        }

        if (xToken.Type == TokenType.AlphaNum) {
          if (xToken.Value == "REG") {
            xToken.Type = TokenType.Register;
            xToken.Value = Parser.RegisterList;
          } else if (xToken.Value == "REG8") {
            xToken.Type = TokenType.Register;
            xToken.Value = Parser.Register8List;
          } else if (xToken.Value == "REG16") {
            xToken.Type = TokenType.Register;
            xToken.Value = Parser.Register16List;
          } else if (xToken.Value == "REG32") {
            xToken.Type = TokenType.Register;
            xToken.Value = Parser.Register32List;
          } else if (xToken.Value == "KEYWORD") {
            xToken.Type = TokenType.Keyword;
            xToken.Value = null;
          } else if (xToken.Value == "ABC123") {
            xToken.Value = null;
          }
        } else if (xToken.Type == TokenType.Register && xNext != null && xNext.Type == TokenType.Comma) {
          var xSB = new StringBuilder();
          while (xToken.Type == TokenType.Register || xToken.Type == TokenType.Comma) {
            xSB.Append(xToken.Value);
            xToken = aTokens[++i];
          }
          xToken.Type = TokenType.Register;
          xToken.Value = xSB.ToString();
        }

        xResult.Add(xToken);
      }

      return xResult;
    }

    protected void AddPattern(string aPattern, CodeFunc aCode) {
      var xParser = new Parser(aPattern, false, true);
      var xTokens = ParsePatterns(xParser.Tokens);

      var xPattern = new Pattern() {
        Tokens = xTokens,
        Hash = xTokens.GetHashCode(),
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
