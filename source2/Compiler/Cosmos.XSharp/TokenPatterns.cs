using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.Compiler.XSharp {
  public class TokenPatterns {
    protected class CodeFunc : Func<Token[], string> { }
    protected Dictionary<TokenPattern, Func<Token[], string>> mList = new Dictionary<TokenPattern, Func<Token[], string>>();

    public TokenPatterns() {
      Add(new TokenType[] { TokenType.LiteralAsm }, delegate(Token[] aTokens) { 
        return "new LiteralAssemblerCode(\"{0}\");"; 
      });
      Add(new TokenType[] { TokenType.Comment }, delegate(Token[] aTokens) { 
        return "new Comment(\"{0}\");"; 
      });

      Add("REG = 123", delegate(Token[] aTokens) { 
        return "new Mov{{ DestinationReg = RegistersEnum.{0}, SourceValue = {2} }};"; 
      });
      Add("REG = REG", delegate(Token[] aTokens) { 
        return "new Mov{{ DestinationReg = RegistersEnum.{0}, SourceReg = RegistersEnum.{2} }};"; 
      });
      Add("REG = REG[0]", delegate(Token[] aTokens) { 
        return "//new ;"; 
      });

      Add("ABC = REG", delegate(Token[] aTokens) { 
        return "//new ;"; 
      });

      // TODO: Allow asm to optimize these to Inc/Dec
      Add("REG + 1", delegate(Token[] aTokens) { 
        return "new Add {{ DestinationReg = RegistersEnum.{0}, SourceValue = {2} }};"; 
      });
      
      Add("REG - 1", delegate(Token[] aTokens) { 
        return "new Sub {{ DestinationReg = RegistersEnum.{0}, SourceValue = {2} }};"; 
      });

      Add(new TokenType[] { TokenType.OpCode }, delegate(Token[] aTokens) {
        return "//new ;"; 
      });
    }

    public string GetCode(TokenType[] aPattern) {
      Func<Token[], string> xAction;
      if (!mList.TryGetValue(new TokenPattern(aPattern), out xAction)) {
        throw new Exception("Token pattern not found.");
      }
      return xAction(new Token[0]);
    }

    public void Add(string aPattern, Func<Token[], string> aCode) {
      mList.Add(new TokenPattern(aPattern), aCode);
    }

    public void Add(TokenType[] aPattern, Func<Token[], string> aCode) {
      mList.Add(new TokenPattern(aPattern), aCode);
    }
  }
}
