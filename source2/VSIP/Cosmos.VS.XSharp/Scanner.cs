using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Package;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.VisualStudio.OLE.Interop;

namespace Cosmos.VS.XSharp {
  internal class Scanner : IScanner {
    IVsTextBuffer mBuffer;
    string mSource;

    public Scanner(IVsTextBuffer aBuffer) {
      mBuffer = aBuffer;
    }

    int i = 0;
    bool IScanner.ScanTokenAndProvideInfoAboutIt(TokenInfo tokenInfo, ref int state) {
      tokenInfo.Type = TokenType.Unknown;
      tokenInfo.Color = TokenColor.Text;
      i++;
      return i < 2;
    
      bool foundToken = false;
      string token = ""; // lex.GetNextToken();
      if (token != null) {
        char firstChar = token[0];
        if (char.IsPunctuation(firstChar)) {
          tokenInfo.Type = TokenType.Operator;
          tokenInfo.Color = TokenColor.Keyword;
        } else if (char.IsNumber(firstChar)) {
          tokenInfo.Type = TokenType.Literal;
          tokenInfo.Color = TokenColor.Number;
        } else {
          tokenInfo.Type = TokenType.Identifier;
          tokenInfo.Color = TokenColor.Identifier;
        }
      }
      return foundToken;
    }

    void IScanner.SetSource(string source, int offset) {
      mSource = source.Substring(offset);
    }
  }
}
