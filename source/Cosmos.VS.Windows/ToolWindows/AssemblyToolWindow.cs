using System;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Imaging;
using Microsoft.VisualStudio.Shell;

namespace Cosmos.VS.Windows.ToolWindows
{
    [Guid(ToolWindowGuid)]
    internal class AssemblyToolWindow : ToolWindowPane2
    {
        public const string ToolWindowGuid = "f019fb29-c2c2-4d27-9abf-739533c939be";

        public AssemblyToolWindow()
        {
            BitmapImageMoniker = KnownMonikers.DisassemblyWindow;
            Caption = "Cosmos Assembly";
            Content = new AssemblyUC();
        }
    }
}
