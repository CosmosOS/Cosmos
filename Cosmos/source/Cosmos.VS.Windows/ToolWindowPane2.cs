using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace Cosmos.VS.Windows
{
    public class ToolWindowPane2 : ToolWindowPane
    {
        public event EventHandler UpdateWindow;

        protected DebuggerUC mUserControl;

        public DebuggerUC UserControl => mUserControl;

        public ToolWindowPane2()
            : base(null)
        {
        }

        protected virtual void OnUpdateWindow(EventArgs e)
        {
            UpdateWindow?.Invoke(this, e);
        }
    }
}