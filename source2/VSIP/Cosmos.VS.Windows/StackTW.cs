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
    [Guid("A64D0FCC-8DCC-439A-9B16-3C43128AAD51")]
    public class StackTW : ToolWindowPane
    {
        public static StackUC mUC;
        public StackTW() : base(null)
        {
            this.Caption = "Cosmos Stack";
            this.BitmapResourceID = 301;
            this.BitmapIndex = 1;

            mUC = new StackUC();
            base.Content = mUC;
        }
    }
}
