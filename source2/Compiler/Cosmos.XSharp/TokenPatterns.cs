using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.Compiler.XSharp {
  public class TokenPatterns : Dictionary<TokenPattern, string> {
    public TokenPatterns() {
      Add(new TokenType[] { TokenType.LiteralAsm }, "new LiteralAssemblerCode(\"{0}\");");
      Add(new TokenType[] { TokenType.Comment }, "new Comment(\"{0}\");");

      Add("REG = 123", "new Mov{{ DestinationReg = RegistersEnum.{0}, SourceValue = {2} }};");
      Add("REG = REG", "new Mov{{ DestinationReg = RegistersEnum.{0}, SourceReg = RegistersEnum.{2} }};");
      Add("REG = REG[0]", "//new ;");

      Add("ABC = REG", "//new ;");

      // TODO: Allow asm to optimize these to Inc/Dec
      Add("REG + 1", "new Add {{ DestinationReg = RegistersEnum.{0}, SourceValue = {2} }};");
      Add("REG - 1", "new Sub {{ DestinationReg = RegistersEnum.{0}, SourceValue = {2} }};");

      Add(new TokenType[] { TokenType.OpCode }, "//new ;");
    }

    public void Add(string aTokenTypes, string aCode) {
      Add(new TokenPattern(aTokenTypes), aCode);
    }

    public void Add(TokenType[] aTokenTypes, string aCode) {
      Add(new TokenPattern(aTokenTypes), aCode);
    }
  }
}
