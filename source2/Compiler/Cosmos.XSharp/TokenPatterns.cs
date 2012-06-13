using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.Compiler.XSharp {
  public class TokenPatterns {
    public delegate string CodeFunc(Token[] aTokens);
    protected Dictionary<TokenPattern, CodeFunc> mList = new Dictionary<TokenPattern, CodeFunc>();

    public TokenPatterns() {
      Add(new TokenType[] { TokenType.LiteralAsm },
        "new LiteralAssemblerCode(\"{0}\");"
      );
      Add(new TokenType[] { TokenType.Comment },
        "new Comment(\"{0}\");"
      );
      Add("Label:" ,
        "new Label(\"{0}\");"
      );

      Add("EAX = 123",
        "new Mov{{ DestinationReg = RegistersEnum.{0}, SourceValue = {2} }};"
      );
      Add("EAX = EAX",
        "new Mov{{ DestinationReg = RegistersEnum.{0}, SourceReg = RegistersEnum.{2} }};"
      );
      Add("EAX = [EAX + 0]",
        "new Mov {{"
          + " DestinationReg = RegistersEnum.{0}"
          + ", SourceReg = RegistersEnum.{3}, SourceIsIndirect = true, SourceDisplacement = {5}"
          + "}};"
      );

      Add("Variable = EAX",
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
      Add("EAX + 1", delegate(Token[] aTokens) {
        if (aTokens[2].Value == "1") {
          return "new Inc {{ DestinationReg = RegistersEnum.{0} }};";
        } else {
          return "new Add {{ DestinationReg = RegistersEnum.{0}, SourceValue = {2} }};";
        }
      });

      Add("EAX - 1", delegate(Token[] aTokens) {
        if (aTokens[2].Value == "1") {
          return "new Dec {{ DestinationReg = RegistersEnum.{0} }};";
        } else {
          return "new Sub {{ DestinationReg = RegistersEnum.{0}, SourceValue = {2} }};";
        }
      });

      Add(new TokenType[] { TokenType.Keyword }, delegate(Token[] aTokens) {
        string xOp = aTokens[0].Value.ToUpper();
        if (xOp == "CALL") {
          return "new Call {{ DestinationLabel = {1} }};";
        } else if (xOp == "GROUP") {
          return "";
        } else if (xOp == "INTERRUPTHANDLER") {
          return "";
        } else if (xOp == "IRET") {
          return "new IRet();";
        } else if (xOp == "POPALL") {
          return "new Popad();";
        } else if (xOp == "PUSHALL") {
          return "new Pushad();";
        } else if (xOp == "PROCEDURE") {
          return "";
        } else {
          throw new Exception("Unrecognized keyword: " + aTokens[0].Value);
        }
      });
    }

    public string GetCode(List<Token> aTokens) {
      var xPattern = aTokens.Select(c => c.Type).ToArray();

      CodeFunc xAction;
      if (!mList.TryGetValue(new TokenPattern(xPattern), out xAction)) {
        throw new Exception("Token pattern not found.");
      }

      string xResult = xAction(aTokens.ToArray());
      return string.Format(xResult, aTokens.Select(c => c.Value).ToArray());
    }

    public void Add(string aPattern, CodeFunc aCode) {
      mList.Add(new TokenPattern(aPattern), aCode);
    }

    public void Add(TokenType[] aPattern, CodeFunc aCode) {
      mList.Add(new TokenPattern(aPattern), aCode);
    }

    public void Add(string aPattern, string aCode) {
      Add(aPattern, delegate(Token[] aTokens) { return aCode; });
    }

    public void Add(TokenType[] aPattern, string aCode) {
      Add(aPattern, delegate(Token[] aTokens) { return aCode; });
    }
  }
}
