using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Windows;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Shell;
using System.ComponentModel.Design;

namespace Cosmos.VS.Windows {
  [Guid("39A8D0A0-26A3-4234-A660-0C4C8BF40FF3")]
  public class InternalTW : ToolWindowPane {
    public static InternalUC mUC;

    public InternalTW()
      : base(null) {
      //this.ToolBar = new CommandID(GuidList.guidAsmToolbarCmdSet, (int)PkgCmdIDList.AsmToolbar);
      this.Caption = "Cosmos Internal";

      this.BitmapResourceID = 301;
      this.BitmapIndex = 1;

      mUC = new InternalUC();
      base.Content = mUC;
    }
  }
}
