using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Windows;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Shell;

namespace Cosmos.Cosmos_VS_Windows
{
    [Guid("CE2A2D0F-0F1B-4A1F-A9AC-5A5F2A5E2C25")]
    public class RegistersTW : ToolWindowPane
    {
        public static RegistersUC mUC;

        public RegistersTW() : base(null)
        {
            this.Caption = "Cosmos x86 Registers";
            this.BitmapResourceID = 301;
            this.BitmapIndex = 1;

            mUC = new RegistersUC();
            base.Content = mUC;
        }
    }
}
