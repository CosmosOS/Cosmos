using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Package;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.VisualStudio.OLE.Interop;
using System.Runtime.InteropServices;

// Walkthrough: Creating a Language Service (MPF)
//   http://msdn.microsoft.com/en-us/library/bb165744
// Language Service Features (MPF)
//   http://msdn.microsoft.com/en-us/library/bb166215
// Syntax Colorizing
//   http://msdn.microsoft.com/en-us/library/bb165041
// Managed Babel
//   http://msdn.microsoft.com/en-us/library/bb165037.aspx

namespace XSharp.VS {
    [Guid(Guids.guidCosmos_VS_XSharpLangSvcString)]
    public class XSharpLanguageService : LanguageService {
    public override string GetFormatFilterList() {
      return "X# files (*.xs)\n*.xs\n";
    }

    private LanguagePreferences mPreferences;
    public override LanguagePreferences GetLanguagePreferences() {
      if (mPreferences == null) {
        mPreferences = new LanguagePreferences(Site, typeof(XSharpLanguageService).GUID, Name);
        mPreferences.Init();
      }
      return mPreferences;
    }

    private Scanner mScanner;
    public override IScanner GetScanner(IVsTextLines aBuffer) {
      if (mScanner == null) {
        mScanner = new Scanner(aBuffer);
      }
      return mScanner;
    }

    public override string Name {
      get { return "X#"; }
    }

    public override AuthoringScope ParseSource(ParseRequest req) {
      return new Parser();
    }
  }
}
