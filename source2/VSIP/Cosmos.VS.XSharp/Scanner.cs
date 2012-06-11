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
    // State argument: http://social.msdn.microsoft.com/Forums/en-US/vsx/thread/38939d76-6f8b-473f-9ee1-fc3ae7b59cce
    bool IScanner.ScanTokenAndProvideInfoAboutIt(TokenInfo aTokenInfo, ref int aState) {
      aTokenInfo.StartIndex = 0;
      aTokenInfo.EndIndex = mSource.Length;
      aTokenInfo.Type = TokenType.Unknown;
      aTokenInfo.Color = TokenColor.Comment;
      i++;
      return i < 2;
    
      bool foundToken = false;
      string token = ""; // lex.GetNextToken();
      if (token != null) {
        char firstChar = token[0];
        if (char.IsPunctuation(firstChar)) {
          aTokenInfo.Type = TokenType.Operator;
          aTokenInfo.Color = TokenColor.Keyword;
        } else if (char.IsNumber(firstChar)) {
          aTokenInfo.Type = TokenType.Literal;
          aTokenInfo.Color = TokenColor.Number;
        } else {
          aTokenInfo.Type = TokenType.Identifier;
          aTokenInfo.Color = TokenColor.Identifier;
        }
      }
      return foundToken;
    }

    void IScanner.SetSource(string aSource, int aOffset) {
      i = 0;
      mSource = aSource.Substring(aOffset);
    }
  }
}
