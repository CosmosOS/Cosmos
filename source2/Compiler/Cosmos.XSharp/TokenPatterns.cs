using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.Compiler.XSharp {
  public class TokenPatterns {
    public delegate string CodeFunc(TokenList aTokens);
    protected Dictionary<TokenPattern, CodeFunc> mPatterns = new Dictionary<TokenPattern, CodeFunc>();
    protected Dictionary<string, CodeFunc> mKeywords = new Dictionary<string, CodeFunc>();
    protected string mGroup;
    protected bool mInIntHandler;

    public TokenPatterns() {
      AddPatterns();
      AddKeywords();
    }

    protected void AddKeywords() {
      AddKeyword("Call", delegate(TokenList aTokens) {
        if (aTokens.Pattern == "Call ABC") {
          return "new Call {{ DestinationLabel = \"{1}\" }};";
        } else if (aTokens.Pattern == "Call .ABC") {
          return "new Call {{ DestinationLabel = \"" + mGroup + "_{2}\" }};";
        }
        return null;
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
          return "new Label(\"{1}\");";
        } else if (aTokens.Pattern == "InterruptHandler .ABC {") {
          return "new Label(\"" + mGroup + "_{2}\");";
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
          return "new Label(\"{1}\");";
        } else if (aTokens.Pattern == "Procedure .ABC {") {
          return "new Label(\"" + mGroup + "_{2}\");";
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
      AddPattern("EAX = [EAX + 0]",
        "new Mov {{"
          + " DestinationReg = RegistersEnum.{0}"
          + ", SourceReg = RegistersEnum.{3}, SourceIsIndirect = true, SourceDisplacement = {5}"
          + "}};"
      );

      AddPattern("Variable = EAX",
        "new Mov {{"
         + "  DestinationRef = Cosmos.Assembler.ElementReference.New(\"{0}\")"
         + " , SourceReg = RegistersEnum.{2}"
         + " }};"
      );

      // TODO: Allow asm to optimize these to Inc/Dec
      AddPattern("EAX + 1", delegate(TokenList aTokens) {
        if (aTokens[2].Value == "1") {
          return "new Inc {{ DestinationReg = RegistersEnum.{0} }};";
        } else {
          return "new Add {{ DestinationReg = RegistersEnum.{0}, SourceValue = {2} }};";
        }
      });

      AddPattern("EAX - 1", delegate(TokenList aTokens) {
        if (aTokens[2].Value == "1") {
          return "new Dec {{ DestinationReg = RegistersEnum.{0} }};";
        } else {
          return "new Sub {{ DestinationReg = RegistersEnum.{0}, SourceValue = {2} }};";
        }
      });

      AddPattern("}", delegate(TokenList aTokens) {
        if (mInIntHandler) {
          return "new IRET();";
        } else {
          return "new Ret();";
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
