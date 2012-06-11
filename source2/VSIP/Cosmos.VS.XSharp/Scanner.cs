using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Package;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.VisualStudio.OLE.Interop;
using XSC = Cosmos.Compiler.XSharp;

namespace Cosmos.VS.XSharp {
  internal class Scanner : IScanner {
    class TokenData {
      public TokenType Type;
      public TokenColor Color;
    }

    IVsTextBuffer mBuffer;
    XSC.Parser mParser;
    int mTokenIdx;
    static Dictionary<XSC.TokenType, TokenData> mTokenMap = new Dictionary<XSC.TokenType, TokenData>();

    static Scanner() {
      mTokenMap.Add(XSC.TokenType.Label, new TokenData { Type = TokenType.Identifier, Color = TokenColor.Identifier });
      mTokenMap.Add(XSC.TokenType.Comment, new TokenData { Type = TokenType.LineComment, Color = TokenColor.Comment });
      mTokenMap.Add(XSC.TokenType.Literal , new TokenData { Type = TokenType.Literal , Color = TokenColor.String });
      mTokenMap.Add(XSC.TokenType.Register , new TokenData { Type = TokenType.Keyword , Color = TokenColor.Keyword });
      mTokenMap.Add(XSC.TokenType.Op , new TokenData { Type = TokenType.Keyword , Color = TokenColor.Keyword });
      mTokenMap.Add(XSC.TokenType.Assignment , new TokenData { Type = TokenType.Operator , Color = TokenColor.Text });
      mTokenMap.Add(XSC.TokenType.ValueNumber , new TokenData { Type = TokenType.Literal , Color = TokenColor.Number });
      mTokenMap.Add(XSC.TokenType.BracketLeft , new TokenData { Type = TokenType.Delimiter , Color = TokenColor.Text });
      mTokenMap.Add(XSC.TokenType.BracketRight , new TokenData { Type = TokenType.Delimiter , Color = TokenColor.Text });
      mTokenMap.Add(XSC.TokenType.Plus , new TokenData { Type = TokenType.Operator , Color = TokenColor.Text });
      mTokenMap.Add(XSC.TokenType.Minus , new TokenData { Type = TokenType.Operator , Color = TokenColor.Text });
      mTokenMap.Add(XSC.TokenType.Inc , new TokenData { Type = TokenType.Operator , Color = TokenColor.Text });
      mTokenMap.Add(XSC.TokenType.Dec , new TokenData { Type = TokenType.Operator , Color = TokenColor.Text });
    }

    public Scanner(IVsTextBuffer aBuffer) {
      mBuffer = aBuffer;
    }

    // State argument: http://social.msdn.microsoft.com/Forums/en-US/vsx/thread/38939d76-6f8b-473f-9ee1-fc3ae7b59cce
    bool IScanner.ScanTokenAndProvideInfoAboutIt(TokenInfo aTokenInfo, ref int aState) {
      if (mTokenIdx == mParser.Tokens.Count) {
        return false;
      }

      var xToken = mParser.Tokens[mTokenIdx];
      mTokenIdx++;

      aTokenInfo.Token = (int)xToken.Type;
      aTokenInfo.StartIndex = xToken.SrcPosStart;
      aTokenInfo.EndIndex = xToken.SrcPosEnd;
      TokenData xType;
      if (mTokenMap.TryGetValue(xToken.Type, out xType)) {
        aTokenInfo.Type = xType.Type;
        aTokenInfo.Color = xType.Color;
      } else {
        aTokenInfo.Type = TokenType.Unknown;
        aTokenInfo.Color = TokenColor.Text;
      }
      return true;
    }

    void IScanner.SetSource(string aSource, int aOffset) {
      mTokenIdx = 0;
      mParser = new XSC.Parser(aSource.Substring(aOffset));
    }
  }
}
