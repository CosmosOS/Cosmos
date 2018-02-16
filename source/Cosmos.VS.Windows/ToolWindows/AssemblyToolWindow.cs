using System;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Shell;

namespace Cosmos.VS.Windows.ToolWindows
{
    [Guid("f019fb29-c2c2-4d27-9abf-739533c939be")]
    internal class AssemblyToolWindow : ToolWindowPane2
    {
        public AssemblyToolWindow()
        {
            //ToolBar = new CommandID(GuidList.guidAsmToolbarCmdSet, (int)PkgCmdIDList.AsmToolbar);
            Caption = "Cosmos Assembly";

            // Set the image that will appear on the tab of the window frame
            // when docked with an other window.
            // The resource ID correspond to the one defined in the resx file
            // while the Index is the offset in the bitmap strip. Each image in
            // the strip being 16x16.
            BitmapResourceID = 301;
            BitmapIndex = 1;

            // This is the user control hosted by the tool window; Note that, even if this class implements IDisposable,
            // we are not calling Dispose on this object. This is because ToolWindowPane calls Dispose on
            // the object returned by the Content property.
            Content = new AssemblyUC();
        }
    }
}
