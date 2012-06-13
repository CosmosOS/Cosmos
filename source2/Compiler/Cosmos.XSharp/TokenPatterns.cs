using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.Compiler.XSharp {
  public class TokenPatterns {
    protected Dictionary<TokenPattern, Func<string>> mList = new Dictionary<TokenPattern, Func<string>>();

    public TokenPatterns() {
      Add(new TokenType[] { TokenType.LiteralAsm }, delegate() { 
        return "new LiteralAssemblerCode(\"{0}\");"; 
      });
      Add(new TokenType[] { TokenType.Comment }, delegate() { 
        return "new Comment(\"{0}\");"; 
      });

      Add("REG = 123", delegate() { 
        return "new Mov{{ DestinationReg = RegistersEnum.{0}, SourceValue = {2} }};"; 
      });
      Add("REG = REG", delegate() { 
        return "new Mov{{ DestinationReg = RegistersEnum.{0}, SourceReg = RegistersEnum.{2} }};"; 
      });
      Add("REG = REG[0]", delegate() { 
        return "//new ;"; 
      });

      Add("ABC = REG", delegate() { 
        return "//new ;"; 
      });

      // TODO: Allow asm to optimize these to Inc/Dec
      Add("REG + 1", delegate() { 
        return "new Add {{ DestinationReg = RegistersEnum.{0}, SourceValue = {2} }};"; 
      });
      
      Add("REG - 1", delegate() { 
        return "new Sub {{ DestinationReg = RegistersEnum.{0}, SourceValue = {2} }};"; 
      });

      Add(new TokenType[] { TokenType.OpCode }, delegate() {
        return "//new ;"; 
      });
    }

    public string GetCode(TokenType[] aPattern) {
      Func<string> xAction;
      if (!mList.TryGetValue(new TokenPattern(aPattern), out xAction)) {
        throw new Exception("Token pattern not found.");
      }
      return xAction();
    }

    public void Add(string aPattern, Func<string> aCode) {
      mList.Add(new TokenPattern(aPattern), aCode);
    }

    public void Add(TokenType[] aPattern, Func<string> aCode) {
      mList.Add(new TokenPattern(aPattern), aCode);
    }
  }
}
