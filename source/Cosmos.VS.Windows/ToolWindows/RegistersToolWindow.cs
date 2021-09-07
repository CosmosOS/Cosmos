using System;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Imaging;
using Microsoft.VisualStudio.Shell;

namespace Cosmos.VS.Windows.ToolWindows
{
    [Guid(ToolWindowGuid)]
    internal class RegistersToolWindow : ToolWindowPane2
    {
        public const string ToolWindowGuid = "ce2a2d0f-0f1b-4a1f-a9ac-5a5f2a5e2c25";

        public RegistersToolWindow()
        {
            BitmapImageMoniker = KnownMonikers.RegistersWindow;
            Caption = "Cosmos Registers";
            Content = new RegistersControl();
        }
    }
}
