using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.Compiler.XSharp {
  public class TokenPatterns {
    public delegate string CodeFunc(TokenList aTokens);
    protected Dictionary<TokenPattern, CodeFunc> mPatterns = new Dictionary<TokenPattern, CodeFunc>();
    protected Dictionary<string, CodeFunc> mKeywords = new Dictionary<string, CodeFunc>();

    public TokenPatterns() {
      AddPatterns();
      AddKeywords();
    }

    protected void AddKeywords() {
      AddKeyword("Call", delegate(TokenList aTokens) {
        if (aTokens.Pattern.Matches("Call ABC")) {
          return "new Call {{ DestinationLabel = {1} }};";
        } else if (aTokens.Pattern.Matches("Call .ABC")) {
          return "new Call {{ DestinationLabel = {1} }};";
        }
        throw new Exception("Unrecognized syntax for keyword: " + aTokens[0].Value);
      });
      AddKeyword("IRet", "new IRet();");
      AddKeyword("PopAll", "new Popad();");
      AddKeyword("PushAll", "new Pushad();");
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
         + "  DestinationRef = Cosmos.Assembler.ElementReference.New(RegistersEnum.{0})"
         + " , DestinationIsIndirect = true"
         + " , SourceValue = value.Value.GetValueOrDefault()"
         + " , SourceRef = value.Reference"
         + " , SourceReg = value.Register"
         + " , SourceIsIndirect = value.IsIndirect"
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

      AddPattern("}",
        ""
      );

      AddPattern(new TokenType[] { TokenType.Keyword, TokenType.AlphaNum }, delegate(TokenList aTokens) {
        string xOp = aTokens[0].Value.ToUpper();
        if (xOp == "CALL") {
          return "new Call {{ DestinationLabel = {1} }};";
        } else if (xOp == "GROUP") {
          return "";
        } else {
          throw new Exception("Unrecognized keyword: " + aTokens[0].Value);
        }
      });

      AddPattern(new TokenType[] { TokenType.Keyword, TokenType.AlphaNum, TokenType.CurlyLeft }, delegate(TokenList aTokens) {
        string xOp = aTokens[0].Value.ToUpper();
        if (xOp == "INTERRUPTHANDLER") {
          return "";
        } else if (xOp == "PROCEDURE") {
          return "";
        } else {
          throw new Exception("Unrecognized keyword: " + aTokens[0].Value);
        }
      });
    }

    public string GetCode(TokenList aTokens) {
      CodeFunc xAction = null;
      if (aTokens[0].Type == TokenType.Keyword) {
        mKeywords.TryGetValue(aTokens[0].Value.ToUpper(), out xAction);
      }
      if (xAction == null) {
        mPatterns.TryGetValue(aTokens.Pattern, out xAction);
        if (xAction == null) {
          throw new Exception("Token pattern not found.");
        }
      }

      string xResult = xAction(aTokens);
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
