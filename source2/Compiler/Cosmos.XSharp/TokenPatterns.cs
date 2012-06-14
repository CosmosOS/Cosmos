using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Cosmos.Compiler.XSharp {
  public class TokenPatterns {
    public delegate string CodeFunc(TokenList aTokens);
    protected Dictionary<TokenPattern, CodeFunc> mPatterns = new Dictionary<TokenPattern, CodeFunc>();
    protected Dictionary<string, CodeFunc> mKeywords = new Dictionary<string, CodeFunc>();
    protected string mGroup;
    protected string mProcedureName;
    protected bool mInIntHandler;

    public TokenPatterns() {
      AddPatterns();
      AddKeywords();
    }

    protected void AddKeywords() {
      AddKeyword("Call", delegate(TokenList aTokens) {
        if (aTokens.Pattern == "Call ABC") {
          return "new Call {{ DestinationLabel = \"" + mGroup + "_{1}\" }};";
        }
        return null;
      });

      AddKeyword("Exit", delegate(TokenList aTokens) {
        return "new Jump {{ DestinationLabel = \"" + mGroup + "_" + mProcedureName + "_Exit\" }};";
      });

      AddKeyword("Group", delegate(TokenList aTokens) {
        if (aTokens.Pattern == "Group ABC") {
          mGroup = aTokens[1].Value;
          return "";
        }
        return null;
      });

      AddKeyword("InterruptHandler", delegate(TokenList aTokens) {
        mInIntHandler = true;
        if (aTokens.Pattern == "InterruptHandler ABC {") {
          mProcedureName = aTokens[1].Value;
          return "new Label(\"" + mGroup + "_{1}\");";
        }
        return null;
      });

      AddKeyword("Jump", delegate(TokenList aTokens) {
        if (aTokens.Pattern == "Jump ABC") {
          return "new Jump {{ DestinationLabel = \"" + mGroup + "_{1}\" }};";
        }
        return null;
      });

      AddKeyword("Return", "new Ret();");
      AddKeyword("ReturnInterrupt", "new IRET();");
      AddKeyword("PopAll", "new Popad();");
      AddKeyword("PushAll", "new Pushad();");

      AddKeyword("Procedure", delegate(TokenList aTokens) {
        mInIntHandler = false;
        if (aTokens.Pattern == "Procedure ABC {") {
          mProcedureName = aTokens[1].Value;
          return "new Label(\"" + mGroup + "_{1}\");";
        }
        return null;
      });
    }

    protected void AddKeyword(string aKeyword, CodeFunc aCode) {
      mKeywords.Add(aKeyword.ToUpper(), aCode);
    }

    protected void AddKeyword(string aKeyword, string aCode) {
      AddKeyword(aKeyword, delegate(TokenList aTokens) {
        return aCode;
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
      AddPattern(new TokenType[] { TokenType.LiteralAsm },
        "new LiteralAssemblerCode(\"{0}\");"
      );
      AddPattern(new TokenType[] { TokenType.Comment },
        "new Comment(\"{0}\");"
      );
      AddPattern("Label:" ,
        "new Label(\"{0}\");"
      );

      AddPattern("EAX = 123",
        "new Mov{{ DestinationReg = RegistersEnum.{0}, SourceValue = {2} }};"
      );
      AddPattern("EAX = EAX",
        "new Mov{{ DestinationReg = RegistersEnum.{0}, SourceReg = RegistersEnum.{2} }};"
      );
      AddPattern("EAX = [EAX]",
        "new Mov {{"
          + " DestinationReg = RegistersEnum.{0}"
          + ", SourceReg = RegistersEnum.{3}, SourceIsIndirect = true"
          + "}};"
      );
      AddPattern("EAX = [EAX + 1]",
        "new Mov {{"
          + " DestinationReg = RegistersEnum.{0}"
          + ", SourceReg = RegistersEnum.{3}, SourceIsIndirect = true, SourceDisplacement = {5}"
          + "}};"
      );

      AddPattern("Variable = EAX", delegate(TokenList aTokens) {
        return "new Mov {{"
         + " DestinationRef = Cosmos.Assembler.ElementReference.New(\"" + mGroup + "_{0}\"), DestinationIsIndirect = true"
         + " , SourceReg = RegistersEnum.{2}"
         + " }};";
      });

      // TODO: Allow asm to optimize these to Inc/Dec
      AddPattern("EAX + 1", delegate(TokenList aTokens) {
        if (IntValue(aTokens[2]) == 1) {
          return "new Inc {{ DestinationReg = RegistersEnum.{0} }};";
        } else {
          return "new Add {{ DestinationReg = RegistersEnum.{0}, SourceValue = {2} }};";
        }
      });

      AddPattern("EAX - 1", delegate(TokenList aTokens) {
        if (IntValue(aTokens[2]) == 1) {
          return "new Dec {{ DestinationReg = RegistersEnum.{0} }};";
        } else {
          return "new Sub {{ DestinationReg = RegistersEnum.{0}, SourceValue = {2} }};";
        }
      });

      AddPattern("}", delegate(TokenList aTokens) {
        var xCode = "new Label(\"" + mGroup + "_" + mProcedureName + "_Exit\");\r\n";
        if (mInIntHandler) {
          return xCode + "new IRET();";
        } else {
          return xCode + "new Ret();";
        }
      });
    }

    public string GetCode(TokenList aTokens) {
      CodeFunc xAction = null;
      string xResult = null;
      if (aTokens[0].Type == TokenType.Keyword) {
        if (mKeywords.TryGetValue(aTokens[0].Value.ToUpper(), out xAction)) {
          xResult = xAction(aTokens);
          if (xResult == null) {
            throw new Exception("Unrecognized syntax for keyword: " + aTokens[0].Value);
          }
        }
      }
      if (xAction == null) {
        if (!mPatterns.TryGetValue(aTokens.Pattern, out xAction)) {
          throw new Exception("Token pattern not found.");
        }
        xResult = xAction(aTokens);
      }

      return string.Format(xResult, aTokens.Select(c => c.Value).ToArray());
    }

    protected void AddPattern(string aPattern, CodeFunc aCode) {
      mPatterns.Add(new TokenPattern(aPattern), aCode);
    }

    protected void AddPattern(TokenType[] aPattern, CodeFunc aCode) {
      mPatterns.Add(new TokenPattern(aPattern), aCode);
    }

    protected void AddPattern(string aPattern, string aCode) {
      AddPattern(aPattern, delegate(TokenList aTokens) { return aCode; });
    }

    protected void AddPattern(TokenType[] aPattern, string aCode) {
      AddPattern(aPattern, delegate(TokenList aTokens) { return aCode; });
    }
  }
}
