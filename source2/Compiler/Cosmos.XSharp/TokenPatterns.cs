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

      Add("REG = 123",
        "new Mov{{ DestinationReg = RegistersEnum.{0}, SourceValue = {2} }};"
      );
      Add("REG = REG",
        "new Mov{{ DestinationReg = RegistersEnum.{0}, SourceReg = RegistersEnum.{2} }};"
      );
      Add("REG = REG[0]",
        "//new ;"
      );

      Add("ABC = REG",
        "//new ;"
      );

      // TODO: Allow asm to optimize these to Inc/Dec
      Add("REG + 1", delegate(Token[] aTokens) {
        if (aTokens[2].Value == "1") {
          return "new Inc {{ DestinationReg = RegistersEnum.{0} }};";
        } else {
          return "new Add {{ DestinationReg = RegistersEnum.{0}, SourceValue = {2} }};";
        }
      });

      Add("REG - 1", delegate(Token[] aTokens) {
        if (aTokens[2].Value == "1") {
          return "new Dec {{ DestinationReg = RegistersEnum.{0} }};";
        } else {
          return "new Sub {{ DestinationReg = RegistersEnum.{0}, SourceValue = {2} }};";
        }
      });

      Add(new TokenType[] { TokenType.OpCode },
        "//new ;"
      );
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
