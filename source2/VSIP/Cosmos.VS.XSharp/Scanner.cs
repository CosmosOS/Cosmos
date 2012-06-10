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
    private IVsTextBuffer m_buffer;
    string m_source;

    public Scanner(IVsTextBuffer buffer) {
      m_buffer = buffer;
    }

    bool IScanner.ScanTokenAndProvideInfoAboutIt(TokenInfo tokenInfo, ref int state) {
      tokenInfo.Type = TokenType.Unknown;
      tokenInfo.Color = TokenColor.Text;
      return true;
    }

    void IScanner.SetSource(string source, int offset) {
      m_source = source.Substring(offset);
    }
  }
}
